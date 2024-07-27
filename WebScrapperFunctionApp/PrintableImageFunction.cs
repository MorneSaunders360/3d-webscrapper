using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebScrapperFunctionApp.Dto;

namespace WebScrapperFunctionApp
{
    public class PrintableImageFunction
    {

        [Function("PrintableImageFunction")]
        public async Task Run([TimerTrigger("0 */2 * * *")] TimerInfo myTimer)
        {
            SentrySdk.CaptureMessage($"C# Timer trigger function executed at: {DateTime.Now}",SentryLevel.Info);

            if (myTimer.ScheduleStatus is not null)
            {
                var elasticsearchService = new ElasticsearchService<Printable>("printables");
                var searchResponseprintable = elasticsearchService.SearchDocuments(s => s
                                              .Size(1000)
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
                        await Task.Delay(2500);
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }
                    
                }
                SentrySdk.CaptureMessage($"C# Timer trigger function finished: {DateTime.Now}", SentryLevel.Info);
            }
        }
    }
}
