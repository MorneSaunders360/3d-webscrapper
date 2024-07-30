using System;
using System.Diagnostics.Metrics;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using CloudProxySharp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebScrapperFunctionApp.Dto;
using WebScrapperFunctionApp.Dto.PrintablesDetial;
using static System.Net.WebRequestMethods;

namespace WebScrapperFunctionApp
{
    public class PrintableFilesFunction
    {

        [Function("PrintableFilesFunction")]
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
        {
            SentrySdk.CaptureMessage($"TimerTrigger - PrintableDetialFunction {DateTime.Now}");

            try
            {

                var elasticsearchService = new ElasticsearchService<Printable>("printables");

                var searchResponseprintable = elasticsearchService.SearchDocuments(s => s
                                            .Size(100)
                                            .Query(q => q
                                                .Bool(b => b
                                                    .Must(m => m
                                                        .QueryString(qs => qs
                                                            .Query("*files.printables.com*")
                                                        )
                                                    )
                                                    .Filter(f => f
                                                        .MatchPhrase(mp => mp
                                                            .Field("printableDetials.zip_data.files.url.keyword")
                                                            .Query("empty")
                                                        )
                                                    )
                                                )
                                            )
                                        );



                int Counter = 0;
                var tasks = searchResponseprintable.Documents.Select(async printable =>
                {
                    try
                    {
                        if (printable.Type.ToLower() == "printables")
                        {
                            var client = new HttpClient();
                            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.printables.com/graphql/");
                            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36 Edg/127.0.0.0");
                            var content = new StringContent("{\"query\":\"query PrintFiles($id: ID!) {\\n  print(id: $id) {\\n\\n    downloadPacks {\\n      id\\n      name\\n      fileSize\\n      fileType\\n      __typename\\n    }\\n    __typename\\n  }\\n}\\n\",\"variables\":{\"id\":\"{Id}\"}}".Replace("{Id}", printable.Id.Replace("_Printables", string.Empty)), null, "application/json");
                            request.Content = content;
                            var response = await client.SendAsync(request);
                            response.EnsureSuccessStatusCode();
                            var responseContent = await response.Content.ReadAsStringAsync();

                            // Parse the JSON response using JObject
                            var json = JObject.Parse(responseContent);
                            var downloadPacks = json["data"]["print"]["downloadPacks"];
                            var downloadPackId = "";

                            foreach (var pack in downloadPacks)
                            {
                                if ((string)pack["fileType"] == "MODEL_FILES")
                                {
                                    downloadPackId = (string)pack["id"];
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(downloadPackId))
                            {
                                var clientDownloadPackLink = new HttpClient();
                                var requestDownloadPackLink = new HttpRequestMessage(HttpMethod.Post, "https://api.printables.com/graphql/");
                                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36 Edg/127.0.0.0");
                                var contentDownloadPackLink = new StringContent("{\"query\":\"mutation GetDownloadLink($id: ID!, $printId: ID!, $fileType: DownloadFileTypeEnum!, $source: DownloadSourceEnum!) {\\n  getDownloadLink(\\n    id: $id\\n    printId: $printId\\n    fileType: $fileType\\n    source: $source\\n  ) {\\n    ok\\n    errors {\\n      field\\n      messages\\n      __typename\\n    }\\n    output {\\n      link\\n      count\\n      ttl\\n      __typename\\n    }\\n    __typename\\n  }\\n}\",\"variables\":{\"id\":\"{fileId}\",\"fileType\":\"pack\",\"printId\":\"{printId}\",\"source\":\"model_detail\"}}".Replace("{fileId}", downloadPackId).Replace("{printId}", printable.Id.Replace("_Printables", string.Empty)), null, "application/json");
                                requestDownloadPackLink.Content = contentDownloadPackLink;
                                var responseDownloadPackLink = await clientDownloadPackLink.SendAsync(requestDownloadPackLink);
                                responseDownloadPackLink.EnsureSuccessStatusCode();
                                var responseBodyDownloadPackLink = await responseDownloadPackLink.Content.ReadAsStringAsync();

                                var jsonDownloadPackLink = JObject.Parse(responseBodyDownloadPackLink);
                                var downloadLink = jsonDownloadPackLink["data"]?["getDownloadLink"]?["output"]?["link"]?.ToString();

                                var updatedFiles = new List<WebScrapperFunctionApp.Dto.File>();
                                updatedFiles.AddRange(await BlobSerivce.UploadZipContent(downloadLink, "stl"));
                                printable.PrintableDetials.Zip_data.Files = updatedFiles;

                                var Images = printable.PrintableDetials.Zip_data.Images.ToList(); // Create a copy to iterate
                                var updatedImages = new List<WebScrapperFunctionApp.Dto.Image>(); // New list to store updated images

                                foreach (var item in Images)
                                {
                                    updatedImages.Add(new WebScrapperFunctionApp.Dto.Image { name = item.name, url = await BlobSerivce.UploadFile(item.url, $"{Guid.NewGuid()}_{Guid.NewGuid()}{Path.GetExtension(item.url)}", "images") });
                                }

                                // Update the original collection after iteration
                                printable.PrintableDetials.Zip_data.Images = updatedImages;

                                Counter++;
                                SentrySdk.CaptureMessage($"Printable Files Uploaded {Counter} {printable?.Id}");
                                Console.WriteLine($"Printable Files Uploaded {Counter}");
                                await elasticsearchService.UpsertDocument(printable, printable.Id).ConfigureAwait(false);
                            }
                           
                        }
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }
                });


                await Task.WhenAll(tasks);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Printable Detial Exception {ex}");
                SentrySdk.CaptureException(ex);

            }
            SentrySdk.CaptureMessage($"TimerTrigger - PrintableDetialFunction Finished{DateTime.Now}");
        }
    }
}
