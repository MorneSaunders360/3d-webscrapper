using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebScrapperFunctionApp.Dto;

namespace WebScrapperFunctionApp
{
    public class PrintableImageFunction
    {

        [Function("PrintableImageFunction")]
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
        {
            SentrySdk.CaptureMessage($"TimerTrigger - PrintableImageFunction {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                var elasticsearchService = new ElasticsearchService<Printable>("printables");
                var searchResponseprintable = elasticsearchService.SearchDocuments(s => s
                                              .Size(1500)
                                              .Query(q => q
                                                  .Bool(b => b
                                                      .Must(m => m
                                                          .Wildcard(w => w
                                                              .Field(f => f.Thumbnail)
                                                              .Value("*files.printables.com*")
                                                          )
                                                      )
                                                  )
                                              )
                                          );

                int Counter = 0;
                var tasks = searchResponseprintable.Documents.Select(async doc =>
                {
                    try
                    {
                        string link = await BlobSerivce.UploadFile(doc.Thumbnail, $"{Guid.NewGuid()}_{Guid.NewGuid()}{Path.GetExtension(doc.Thumbnail)}", "images");
                        if (!string.IsNullOrEmpty(link))
                        {
                            doc.Thumbnail = link;
                            await elasticsearchService.UpsertDocument(doc, doc.Id);
                            Counter++;
                            SentrySdk.CaptureMessage($"Printable Image Uploaded {Counter}");
                            Console.WriteLine($"Printable Image Uploaded {Counter}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }
                });

                await Task.WhenAll(tasks);
                SentrySdk.CaptureMessage($"TimerTrigger - PrintableDetailFunction Finished {DateTime.Now}");
            }

        }
    }
}
