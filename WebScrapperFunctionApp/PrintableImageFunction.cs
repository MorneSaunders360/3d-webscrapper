using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebScrapperFunctionApp.Dto;

namespace WebScrapperFunctionApp
{
    public class PrintableImageFunction
    {

        [Function("PrintableImageFunction")]
        public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
        {
            SentrySdk.CaptureMessage($"C# Timer trigger function Printable Image executed at: {DateTime.Now}", SentryLevel.Info);

            if (myTimer.ScheduleStatus is not null)
            {
                var elasticsearchService = new ElasticsearchService<Printable>("printables");
                var searchResponseprintable = elasticsearchService.SearchDocuments(s => s
                                              .Size(500)
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
                foreach (var doc in searchResponseprintable.Documents.ToList())
                {
                    try
                    {
                        string link = await BlobSerivce.UploadFile(doc.Thumbnail, $"{Guid.NewGuid()}_{Guid.NewGuid()}.{Path.GetExtension(doc.Thumbnail)}", "images");
                        if (string.IsNullOrEmpty(link))
                        {
                            doc.Thumbnail = link;
                            await elasticsearchService.UpsertDocument(doc, doc.Id);
                        }
                        SentrySdk.CaptureMessage($"Printable Image Uploaded: {Counter++}", SentryLevel.Info);
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }

                }
                SentrySdk.CaptureMessage($"C# Timer trigger function Printable Image finshed at: {DateTime.Now}", SentryLevel.Info);
            }
        }
    }
}
