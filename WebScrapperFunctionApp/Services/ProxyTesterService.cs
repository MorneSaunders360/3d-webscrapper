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
        private static string _testUrl = "http://www.google.com";
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
                UseProxy = true
            };

            using var httpClient = new HttpClient(handler);

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(_testUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Proxy {proxy.Url} is working. Status code: {response.StatusCode}");
                    proxy.IsValid = true;
                }
                else
                {
                    proxy.IsValid = false;
                    Console.WriteLine($"Proxy {proxy.Url} returned a non-success status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                proxy.IsValid = false;
                Console.WriteLine($"Error occurred with proxy {proxy.Url}: {ex.Message}");
            }
            return proxy;
        }
    }

}
