using System;
using Aspose.ThreeD.Shading;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Minio.DataModel;
using Nest;
using Newtonsoft.Json;
using WebScrapperFunctionApp.Dto;
using WebScrapperFunctionApp.Dto.Printables;
using WebScrapperFunctionApp.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebScrapperFunctionApp
{
    public class ProxyFinderFunction
    {

        [Function("ProxyFinderFunction")]
        public async Task Run([TimerTrigger("0 */59 * * * *")] TimerInfo myTimer)
        {
            SentrySdk.CaptureMessage($"TimerTrigger - ProxyFinderFunction {DateTime.Now}");
            if (myTimer.ScheduleStatus is not null)
            {
                var elasticsearchService = new ElasticsearchService<ProxyInfo>("proxies");

                

                try
                {
                    // Fetch existing proxies from Elasticsearch
                    var existingProxies = await elasticsearchService.SearchAllDocumentsAsync(); // Implement this method to fetch all proxies
                    List<ProxyInfo> validExistingProxies = await ProxyTesterService.TestProxiesAsync(existingProxies);
                    // If the count of valid proxies is below 5, fetch new proxies
                    if (validExistingProxies.Where(x=>x.IsValid).ToList().Count < 5)
                    {
                        var client = new HttpClient();
                        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.proxyscrape.com/v2/?request=displayproxies&protocol=http&timeout=1000&country=all&ssl=no&anonymity=all");
                        var response = await client.SendAsync(request);

                        var json = await response.Content.ReadAsStringAsync();
                        string[] proxyArray = json.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

                        var proxiesToTest = proxyArray.Select(proxy => new ProxyInfo
                        {
                            Id = Guid.NewGuid(),
                            Url = proxy,
                            IsValid = false
                        }).ToList();

                        // Test all proxies and get the result including response times
                        List<ProxyInfo> testedProxies = await ProxyTesterService.TestProxiesAsync(proxiesToTest);

                        // Sort proxies by response time and select the top 15
                        var topProxies = testedProxies
                            .Where(proxy => proxy.IsValid) // Only include valid proxies
                            .OrderBy(proxy => proxy.ResponseTime) // Assuming ResponseTime is a property of ProxyInfo
                            .Take(15)
                            .ToList();

                        foreach (var item in existingProxies)
                        {
                            elasticsearchService.DeleteDocument(DocumentPath<ProxyInfo>.Id(item));
                        }

                        // Add only the top 15 fastest proxies
                        foreach (var proxy in topProxies)
                        {
                            await elasticsearchService.UpsertDocument(proxy, proxy.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }



                SentrySdk.CaptureMessage($"TimerTrigger - ProxyFinderFunction Finished{DateTime.Now}");
            }
        }
    }
}
