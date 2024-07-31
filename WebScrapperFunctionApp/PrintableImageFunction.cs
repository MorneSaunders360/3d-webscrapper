using System;
using System.Net.Sockets;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebScrapperFunctionApp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace WebScrapperFunctionApp
{
    public class PrintableImageFunction
    {
        public static async Task<IPAddress?> GetExternalIpAddress()
        {
            var externalIpString = (await new HttpClient().GetStringAsync("http://icanhazip.com"))
                .Replace("\\r\\n", "").Replace("\\n", "").Trim();
            if (!IPAddress.TryParse(externalIpString, out var ipAddress)) return null;
            return ipAddress;
        }

        [Function("PrintableImageFunctionHttp")]
        public async Task<IActionResult> PrintableImageFunctionHttp([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            int Size = int.Parse(req.Query["Size"]);
            Console.WriteLine(await GetExternalIpAddress());
            var elasticsearchService = new ElasticsearchService<Printable>("printables");
            var searchResponseprintable = elasticsearchService.SearchDocuments(s => s
                                          .Size(Size)
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
            Console.WriteLine($"Found Printable Image For Processsing {searchResponseprintable.Documents.Count()}");
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
                        Console.WriteLine($"Printable Image Uploaded {Counter}");
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            });

            await Task.WhenAll(tasks);
            return new OkObjectResult($"TimerTrigger - PrintableDetailFunction Finished {DateTime.Now}");
        }
    }
}
