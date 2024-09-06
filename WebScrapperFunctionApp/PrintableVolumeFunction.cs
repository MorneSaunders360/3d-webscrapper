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
using System.Linq;

namespace WebScrapperFunctionApp
{
    public class PrintableVolumeFunction
    {
        public static async Task<IPAddress?> GetExternalIpAddress()
        {
            var externalIpString = (await new HttpClient().GetStringAsync("http://icanhazip.com"))
                .Replace("\\r\\n", "").Replace("\\n", "").Trim();
            if (!IPAddress.TryParse(externalIpString, out var ipAddress)) return null;
            return ipAddress;
        }

        [Function("PrintableVolumeFunctionFunctionHttp")]
        public async Task<IActionResult> PrintableVolumeFunctionFunctionHttp([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
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
            Console.WriteLine($"Found Printable Volume For Processsing {dtRequest.Count()}");
            List<Printable> dtResponse = new List<Printable>();
            var tasks = dtRequest.Select(async doc =>
            {
                try
                {
                    var dimensionsAndVolumeResponses = await STLProcessorService.ProcessSTLFiles(doc.PrintableDetials.Zip_data.Files.Select(x => x.url).ToList());
                    if (dimensionsAndVolumeResponses != null)
                    {
                        var VolumeSum = dimensionsAndVolumeResponses.Sum(x => x.Volume);
                        if (VolumeSum != 0)
                        {
                            doc.PrintableDetials.Volume = VolumeSum;
                            if (doc.PrintableDetials.Zip_data.Files.Count >= 1)
                            {
                                foreach (var item in doc.PrintableDetials.Zip_data.Files)
                                {
                                    var finditem = dimensionsAndVolumeResponses.FirstOrDefault(x => x.Url == item.url);
                                    if (finditem != null)
                                    {
                                        item.volume = finditem.Volume;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var item in doc.PrintableDetials.Zip_data.Files)
                                {
                                    item.volume = VolumeSum;
                                }
                            }
                            dtResponse.Add(doc);
                            Counter++;
                            Console.WriteLine($"Printable Volume Updated {Counter}_{doc.Id}");
                        }

                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            });

            await Task.WhenAll(tasks);
            Func<Printable, string> idSelector = doc => doc.Id.ToString();
            await elasticsearchService.BulkUpsertDocuments(dtResponse, idSelector).ConfigureAwait(false);
            return new OkObjectResult($"TimerTrigger - PrintableVolumeFunctionFunctionHttp Finished {DateTime.Now}");
        }
    }
}
