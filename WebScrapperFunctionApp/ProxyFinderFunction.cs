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
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
        {
            var transaction = SentrySdk.StartTransaction("TimerTrigger - ProxyFinderFunction", "ProxyFinderFunction");
            SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);
            if (myTimer.ScheduleStatus is not null)
            {
                var elasticsearchService = new ElasticsearchService<ProxyInfo>("proxies");
                var searchResponseprintable = await elasticsearchService.SearchAllDocumentsAsync();


                try
                {
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get, "https://api.proxyscrape.com/v2/?request=displayproxies&protocol=http&timeout=1000&country=all&ssl=no&anonymity=all"); ;
                    var response = await client.SendAsync(request);

                    var json = await response.Content.ReadAsStringAsync();
                    string[] proxyArray = json.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var proxy in proxyArray.ToList())
                    {
                        searchResponseprintable.Add(new ProxyInfo
                        {
                            Id = Guid.NewGuid(),
                            Url = proxy,
                            IsValid = false
                        });
                    }
                    List<ProxyInfo> TestedProxies = await ProxyTesterService.TestProxiesAsync(searchResponseprintable);
                    foreach (var item in TestedProxies)
                    {
                        if (item.IsValid)
                        {
                           await elasticsearchService.UpsertDocument(item, item.Id);
                        }
                        else
                        {
                            elasticsearchService.DeleteDocument(DocumentPath<ProxyInfo>.Id(item));
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
                transaction.Finish();
            }
        }
    }
}
