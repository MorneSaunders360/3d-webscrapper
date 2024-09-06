using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using CloudProxySharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebScrapperFunctionApp.Dto;
using static System.Net.WebRequestMethods;

namespace WebScrapperFunctionApp
{

    public class PrintableDetialFunction
    {
        public static async Task<IPAddress?> GetExternalIpAddress()
        {
            var externalIpString = (await new HttpClient().GetStringAsync("http://icanhazip.com"))
                .Replace("\\r\\n", "").Replace("\\n", "").Trim();
            if (!IPAddress.TryParse(externalIpString, out var ipAddress)) return null;
            return ipAddress;
        }

        [Function("PrintableDetialFunctionHttp")]
        public async Task<IActionResult> PrintableDetialFunctionHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req)
        {
            string reqContent = await new StreamReader(req.Body).ReadToEndAsync();
            Printable AppConfigReponse = new();
            List<Printable> dtRequest;
            Console.WriteLine(await GetExternalIpAddress());
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
            Console.WriteLine($"Found Printable Detial For Processsing {dtRequest.Count()}");

            List<Printable> dtResponse = new List<Printable>();

            var tasks = dtRequest.Select(async printable =>
            {
                try
                {
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.thingiverse.com/things/{printable.Id.Replace("_Printables", string.Empty)}");
                    request.Headers.Add("accept", "application/json");
                    request.Headers.Add("Authorization", "Bearer ae49e79d6656ef8b0aa86fbc322706ec");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var reponseContemt = await response.Content.ReadAsStringAsync();
                            var PrintablesDetialApi = JsonConvert.DeserializeObject<ThingiverseItemDetialResponse>(reponseContemt);
                            printable.CreatedDate = DateTime.Now;
                            printable.PrintableDetials = new PrintableDetials()
                            {
                                Creator = PrintablesDetialApi.Creator,
                                Name = PrintablesDetialApi.Name,
                                License = "Unknown",
                                Public_url = PrintablesDetialApi.PublicUrl,
                                Thumbnail = PrintablesDetialApi.Thumbnail,
                                Url = PrintablesDetialApi.PublicUrl,
                                Volume = 0,

                            };
                            printable.PrintableDetials.Zip_data = PrintablesDetialApi.ZipData;
                            dtResponse.Add(printable);
                            Console.WriteLine($"Printable Detail Updated {Counter++}_{printable.Id}");
                        }
                        catch (Exception ex)
                        {

                        }
                    
                    }
                }
                catch (Exception ex)
                {

     
                }
            
            });

            await Task.WhenAll(tasks);
            Func<Printable, string> idSelector = doc => doc.Id.ToString();
            await elasticsearchService.BulkUpsertDocuments(dtResponse, idSelector).ConfigureAwait(false);
            return new OkObjectResult($"TimerTrigger - PrintableDetialFunction Finished {DateTime.Now}");
        }
    }
}
