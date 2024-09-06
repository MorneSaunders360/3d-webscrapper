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
using WebScrapperFunctionApp.Services;
using CommunityToolkit.Mvvm.DependencyInjection;

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
        public async Task<IActionResult> PrintableImageFunctionHttp([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req)
        {

            var useProxy = await ProxyTesterService.GetActiveProxyAsync();
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = new WebProxy(useProxy.Url),
                UseProxy = true
            };
            var client = new HttpClient(httpClientHandler);
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
            Console.WriteLine($"Found Printable Image For Processsing {dtRequest.Count()}");
            List<Printable> dtResponse = new List<Printable>();
            foreach (Printable doc in dtRequest)
            {
                try
                {
                    string link = await BlobSerivce.UploadFile(doc.Thumbnail, $"{doc.Id}_{Guid.NewGuid()}_{Guid.NewGuid()}{Path.GetExtension(doc.Thumbnail)}", "images");
                    if (!string.IsNullOrEmpty(link))
                    {
                        doc.Thumbnail = link;
                        dtResponse.Add(doc);

                        Counter++;
                        Console.WriteLine($"Printable Image Uploaded {Counter}_{doc.Id}");
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }

            Func<Printable, string> idSelector = doc => doc.Id.ToString();
            await elasticsearchService.BulkUpsertDocuments(dtResponse, idSelector).ConfigureAwait(false);
            return new OkObjectResult($"TimerTrigger - PrintableDetailFunction Finished {DateTime.Now}");
        }
    }
}
