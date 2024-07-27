using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebScrapperFunctionApp.Dto;

namespace WebScrapperFunctionApp.Services
{

    public class ProxyTesterService
    {
        private static string _testUrl = "https://3dfunction.houselabs.co.za/api/GetHealth";
        public static async Task<List<ProxyInfo>> TestProxiesAsync(List<ProxyInfo> searchResponseprintable)
        {
            var tasks = new List<Task>();

            foreach (var proxy in searchResponseprintable)
            {
                tasks.Add(TestProxyAsync(proxy));
            }

            await Task.WhenAll(tasks);

            return searchResponseprintable;
        }

        private static async Task<ProxyInfo> TestProxyAsync(ProxyInfo proxy)
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(proxy.Url),
                UseProxy = true,
            };

            try
            {
                var httpClient = new HttpClient(handler);
                var request = new HttpRequestMessage(HttpMethod.Get, _testUrl);
                request.Headers.Add("Accept", "application/json");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start measuring time
                var response = await httpClient.SendAsync(request);
                stopwatch.Stop(); // Stop measuring time

                var content = await response.Content.ReadAsStringAsync();
                proxy.ResponseTime = stopwatch.ElapsedMilliseconds; // Set response time

                if (response.IsSuccessStatusCode)
                {
                    if (IsValidContent(content))
                    {
                        Console.WriteLine($"Proxy {proxy.Url} is working. Status code: {response.StatusCode}. Response time: {proxy.ResponseTime} ms");
                        proxy.IsValid = true;
                    }
                    else
                    {
                        Console.WriteLine($"Proxy {proxy.Url} returned invalid content. Response time: {proxy.ResponseTime} ms");
                        proxy.IsValid = false;
                    }
                }
                else
                {
                    Console.WriteLine($"Proxy {proxy.Url} returned a non-success status code: {response.StatusCode}. Response time: {proxy.ResponseTime} ms");
                    proxy.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred with proxy {proxy.Url}: {ex.Message}. Response time: {proxy.ResponseTime} ms");
                proxy.IsValid = false;
            }
            return proxy;
        }


        private static bool IsValidContent(string content)
        {
            // Implement content validation logic here
            // For example, check if content contains expected keywords or structure
            return !string.IsNullOrWhiteSpace(content) && content.Contains("true");
        }

    }

}
