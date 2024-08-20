using System;
using System.Net.Sockets;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebScrapperFunctionApp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WebScrapperFunctionApp
{
    public class PrintableDetialImageAndFilesFunction
    {
        public static async Task<IPAddress?> GetExternalIpAddress()
        {
            var externalIpString = (await new HttpClient().GetStringAsync("http://icanhazip.com"))
                .Replace("\\r\\n", "").Replace("\\n", "").Trim();
            if (!IPAddress.TryParse(externalIpString, out var ipAddress)) return null;
            return ipAddress;
        }

        [Function("PrintableDetialImageAndFilesFunctionHttp")]
        public async Task<IActionResult> PrintableDetialImageAndFilesFunctionHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req)
        {
            string reqContent = await new StreamReader(req.Body).ReadToEndAsync();
            Printable AppConfigReponse = new();
            List<Printable> dtRequest;
            try
            {
                dtRequest = JsonConvert.DeserializeObject<List<Printable>>(reqContent);

                if (dtRequest is null)
                {
                    return new BadRequestObjectResult(dtRequest);
                }
            }
            catch (Exception)
            {
                return new BadRequestObjectResult(AppConfigReponse);
            }
            var elasticsearchService = new ElasticsearchService<Printable>("printables");
            int Counter = 0;
            Console.WriteLine($"Found Printable Detial files For Processsing {dtRequest.Count()}");
            var tasks = dtRequest.Select(async doc =>
            {
                try
                {
                    if (doc.PrintableDetials != null)
                    {
                        var updatedFiles = new List<WebScrapperFunctionApp.Dto.File>();
                        var updatedImages = new List<WebScrapperFunctionApp.Dto.Image>();
                        foreach (var file in doc.PrintableDetials.Zip_data.Files)
                        {
                            string link = await BlobSerivce.UploadFile(file.url, $"{doc.Id}_{Guid.NewGuid()}_{Guid.NewGuid()}{Path.GetExtension(file.url)}", "stl");
                            if (!string.IsNullOrEmpty(link))
                            {
                                file.url = link;
                                updatedFiles.Add(file);
                            }
                        }
                        foreach (var image in doc.PrintableDetials.Zip_data.Images)
                        {
                            string link = await BlobSerivce.UploadFile(image.url, $"{doc.Id}_{Guid.NewGuid()}_{Guid.NewGuid()}{Path.GetExtension(image.url)}", "images");
                            if (!string.IsNullOrEmpty(link))
                            {
                                image.url = link;
                                updatedImages.Add(image);
                            }
                        }
                        if (updatedFiles.Count > 0 && updatedImages.Count > 0)
                        {
                            Counter++;
                            doc.PrintableDetials.Zip_data.Files = updatedFiles;
                            doc.PrintableDetials.Zip_data.Images = updatedImages;
                            Console.WriteLine($"Printable Detial File & Images Uploaded {Counter}");
                            await elasticsearchService.UpsertDocument(doc, doc.Id);
                        }

                    }

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            });

            await Task.WhenAll(tasks);
            return new OkObjectResult($"TimerTrigger - PrintableDetialImageAndFilesFunctionHttp Finished {DateTime.Now}");
        }
    }
}
