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
using Aspose.ThreeD.Shading;
using AndreasReitberger.Print3d.Models;
using AndreasReitberger.Print3d.Enums;
using AndreasReitberger.Print3d.Models.MaterialAdditions;

namespace WebScrapperFunctionApp
{
    public class PrintableCostFunction
    {
        public static async Task<IPAddress?> GetExternalIpAddress()
        {
            var externalIpString = (await new HttpClient().GetStringAsync("http://icanhazip.com"))
                .Replace("\\r\\n", "").Replace("\\n", "").Trim();
            if (!IPAddress.TryParse(externalIpString, out var ipAddress)) return null;
            return ipAddress;
        }

        [Function("PrintableCostFunctionHttp")]
        public async Task<IActionResult> PrintableCostFunctionHttp([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
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
            var elasticsearchServicematerial3d = new ElasticsearchService<Material3d>("material3d");
            List<Material3d> Material3dList = await elasticsearchServicematerial3d.SearchAllDocumentsAsync();

            var elasticsearchService = new ElasticsearchService<Printable>("printables");
            int Counter = 0;
            List<Printable> dtResponse = new List<Printable>();
            Console.WriteLine($"Found Printable Cost For Processsing {dtRequest.Count()}");
            var tasks = dtRequest.Select(async doc =>
            {
                try
                {
                    var _calculation = new Calculation3dEnhanced();
                    Print3dInfo print3DInfo = new Print3dInfo();
                    print3DInfo.File = new File3d()
                    {
                        Volume = doc.PrintableDetials.Volume,
                        Quantity = 1,
                    };

                    Material3dUsage material3DUsage = new Material3dUsage();
                    material3DUsage.Material = Material3dList.FirstOrDefault();
                    print3DInfo.MaterialUsages = new List<Material3dUsage>() { material3DUsage };

                    _calculation.PrintInfos = new List<Print3dInfo>() { print3DInfo };
                    _calculation.CalculateCosts();
                    double totalCosts = _calculation.GetTotalCosts();
                    if (totalCosts != 0)
                    {
                        doc.PrintableDetials.Cost = totalCosts;
                        dtResponse.Add(doc);
                        Counter++;
                        Console.WriteLine($"Printable Cost Updated {Counter}_{doc.Id}");

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
            return new OkObjectResult($"TimerTrigger - PrintableCostFunctionHttp Finished {DateTime.Now}");
        }
    }
}
