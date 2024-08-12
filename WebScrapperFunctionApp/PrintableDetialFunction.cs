using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using CloudProxySharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebScrapperFunctionApp.Dto;
using WebScrapperFunctionApp.Dto.PrintablesDetial;
using static System.Net.WebRequestMethods;

namespace WebScrapperFunctionApp
{

    public class PrintableDetialFunction
    {


        [Function("PrintableDetialFunctionHttp")]
        public async Task<IActionResult> PrintableDetialFunctionHttp([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {

            try
            {
                var elasticsearchService = new ElasticsearchService<Printable>("printables");
                int Size = int.Parse(req.Query["Size"]);
                var searchResponseprintable = elasticsearchService.SearchDocuments(s => s
                                              .Size(Size)
                                              .Query(q => q
                                                  .Bool(b => b
                                                      .MustNot(mn => mn
                                                          .Exists(e => e
                                                              .Field(f => f.PrintableDetials)
                                                          )
                                                      )
                                                  )
                                              )
                                          );


                Console.WriteLine($"Found Printable Detial For Processsing {searchResponseprintable.Documents.Count()}");
                foreach (var printable in searchResponseprintable.Documents)
                {
                    try
                    {
                        if (printable.Type.ToLower() == "printables")
                        {
                            var client = new HttpClient();
                            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.printables.com/graphql/");
                            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Edg/126.0.0.0");
                            var JsonContent = "{\"query\":\"query PrintProfile($id: ID!, $loadPurchase: Boolean!) {\\n  print(id: $id) {\\n    ...PrintDetailFragment\\n    price\\n    user {\\n      billingAccountType\\n      lowestTierPrice\\n      highlightedModels {\\n        models {\\n          ...PrintListFragment\\n          __typename\\n        }\\n        featured\\n        __typename\\n      }\\n      __typename\\n    }\\n    purchaseDate @include(if: $loadPurchase)\\n    paidPrice @include(if: $loadPurchase)\\n    __typename\\n  }\\n}\\n\\nfragment PrintDetailFragment on PrintType {\\n  id\\n  slug\\n  name\\n  authorship\\n  remixDescription\\n  premium\\n  price\\n  excludeCommercialUsage\\n  eduProject {\\n    id\\n    subject {\\n      id\\n      name\\n      slug\\n      __typename\\n    }\\n    language {\\n      id\\n      name\\n      __typename\\n    }\\n    free\\n    timeDifficulty\\n    audienceAge\\n    complexity\\n    equipment {\\n      id\\n      name\\n      __typename\\n    }\\n    suitablePrinters {\\n      id\\n      name\\n      __typename\\n    }\\n    organisation\\n    authors\\n    targetGroupFocus\\n    knowledgeAndSkills\\n    objectives\\n    equipmentDescription\\n    timeSchedule\\n    workflow\\n    approved\\n    datePublishRequested\\n    __typename\\n  }\\n  user {\\n    ...AvatarUserFragment\\n    isFollowedByMe\\n    canBeFollowed\\n    publishedPrintsCount\\n    premiumPrintsCount\\n    designer\\n    stripeAccountActive\\n    membership {\\n      currentTier {\\n        id\\n        name\\n        benefits {\\n          id\\n          title\\n          benefitType\\n          description\\n          __typename\\n        }\\n        __typename\\n      }\\n      __typename\\n    }\\n    __typename\\n  }\\n  ratingAvg\\n  myRating\\n  ratingCount\\n  description\\n  category {\\n    id\\n    path {\\n      id\\n      name\\n      nameEn\\n      storeName\\n      description\\n      storeDescription\\n      __typename\\n    }\\n    __typename\\n  }\\n  mmu\\n  modified\\n  firstPublish\\n  datePublished\\n  dateCreatedThingiverse\\n  nsfw\\n  summary\\n  likesCount\\n  makesCount\\n  liked\\n  printDuration\\n  numPieces\\n  weight\\n  nozzleDiameters\\n  usedMaterial\\n  layerHeights\\n  materials {\\n    id\\n    name\\n    __typename\\n  }\\n  dateFeatured\\n  downloadCount\\n  displayCount\\n  filesCount\\n  privateCollectionsCount\\n  publicCollectionsCount\\n  pdfFilePath\\n  commentCount\\n  userGcodeCount\\n  remixCount\\n  canBeRated\\n  printer {\\n    id\\n    name\\n    __typename\\n  }\\n  image {\\n    ...ImageSimpleFragment\\n    __typename\\n  }\\n  images {\\n    ...ImageSimpleFragment\\n    __typename\\n  }\\n  tags {\\n    name\\n    id\\n    __typename\\n  }\\n  thingiverseLink\\n  filesType\\n  license {\\n    id\\n    disallowRemixing\\n    __typename\\n  }\\n  remixParents {\\n    ...remixParentDetail\\n    __typename\\n  }\\n  gcodes {\\n    id\\n    name\\n    fileSize\\n    filePreviewPath\\n    __typename\\n  }\\n  stls {\\n    id\\n    name\\n    fileSize\\n    filePreviewPath\\n    __typename\\n  }\\n  slas {\\n    id\\n    name\\n    fileSize\\n    filePreviewPath\\n    __typename\\n  }\\n  ...LatestCompetitionResult\\n  competitions {\\n    id\\n    name\\n    slug\\n    description\\n    isOpen\\n    __typename\\n  }\\n  competitionResults {\\n    placement\\n    competition {\\n      id\\n      name\\n      slug\\n      printsCount\\n      openFrom\\n      openTo\\n      __typename\\n    }\\n    __typename\\n  }\\n  __typename\\n}\\n\\nfragment AvatarUserFragment on UserType {\\n  id\\n  publicUsername\\n  avatarFilePath\\n  handle\\n  company\\n  verified\\n  badgesProfileLevel {\\n    profileLevel\\n    __typename\\n  }\\n  __typename\\n}\\n\\nfragment ImageSimpleFragment on PrintImageType {\\n  id\\n  filePath\\n  rotation\\n  __typename\\n}\\n\\nfragment remixParentDetail on PrintRemixType {\\n  id\\n  parentPrintId\\n  parentPrintName\\n  parentPrintAuthor {\\n    id\\n    publicUsername\\n    verified\\n    handle\\n    __typename\\n  }\\n  parentPrint {\\n    id\\n    name\\n    slug\\n    datePublished\\n    image {\\n      ...ImageSimpleFragment\\n      __typename\\n    }\\n    premium\\n    authorship\\n    license {\\n      id\\n      name\\n      disallowRemixing\\n      __typename\\n    }\\n    eduProject {\\n      id\\n      __typename\\n    }\\n    __typename\\n  }\\n  url\\n  urlAuthor\\n  urlImage\\n  urlTitle\\n  urlLicense {\\n    id\\n    name\\n    disallowRemixing\\n    __typename\\n  }\\n  urlLicenseText\\n  __typename\\n}\\n\\nfragment LatestCompetitionResult on PrintType {\\n  latestCompetitionResult {\\n    placement\\n    competitionId\\n    __typename\\n  }\\n  __typename\\n}\\n\\nfragment PrintListFragment on PrintType {\\n  id\\n  name\\n  slug\\n  ratingAvg\\n  likesCount\\n  liked\\n  datePublished\\n  dateFeatured\\n  firstPublish\\n  downloadCount\\n  category {\\n    id\\n    path {\\n      id\\n      name\\n      __typename\\n    }\\n    __typename\\n  }\\n  modified\\n  image {\\n    ...ImageSimpleFragment\\n    __typename\\n  }\\n  nsfw\\n  premium\\n  price\\n  user {\\n    ...AvatarUserFragment\\n    __typename\\n  }\\n  ...LatestCompetitionResult\\n  __typename\\n}\",\"variables\":{\"id\":\"{Id}\",\"loadPurchase\":false}}";
                            var content = new StringContent(JsonContent.Replace("{Id}", printable.Id.Replace("_Printables", string.Empty)), null, "application/json");
                            request.Content = content;
                            var response = await client.SendAsync(request);
                            if (response.IsSuccessStatusCode)
                            {
                                PrintablesDetialApi PrintablesDetialApi = JsonConvert.DeserializeObject<PrintablesDetialApi>(await response.Content.ReadAsStringAsync());
                                string ThumbnailLink = await BlobSerivce.UploadFile("https://files.printables.com/" + PrintablesDetialApi.Data.Print.Image.FilePath, $"{printable.Id}_{Guid.NewGuid()}_{Guid.NewGuid()}{Path.GetExtension(PrintablesDetialApi.Data.Print.Image.FilePath)}", "images");
                                printable.PrintableDetials = new PrintableDetials()
                                {
                                    Creator = new Creator { FirstName = PrintablesDetialApi.Data.Print.User.PublicUsername, LastName = PrintablesDetialApi.Data.Print.User.PublicUsername, Name = PrintablesDetialApi.Data.Print.User.Handle },
                                    Name = PrintablesDetialApi.Data.Print.Name,
                                    License = "Unknown",
                                    Public_url = $"https://www.printables.com/model/{printable.Id.Replace("_Printables", string.Empty)}",
                                    Thumbnail = ThumbnailLink,
                                    Url = $"https://www.printables.com/model/{printable.Id.Replace("_Printables", string.Empty)}",
                                    Volume = 0,

                                };
                                printable.PrintableDetials.Zip_data = new ZipData() { Files = new List<WebScrapperFunctionApp.Dto.File>(), Images = new List<WebScrapperFunctionApp.Dto.Image>() };
                                if (printable.Type.ToLower() == "printables")
                                {
                                    var clientPack = new HttpClient();
                                    var requestPack = new HttpRequestMessage(HttpMethod.Post, "https://api.printables.com/graphql/");
                                    requestPack.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36 Edg/127.0.0.0");
                                    var contentPack = new StringContent("{\"query\":\"query PrintFiles($id: ID!) {\\n  print(id: $id) {\\n\\n    downloadPacks {\\n      id\\n      name\\n      fileSize\\n      fileType\\n      __typename\\n    }\\n    __typename\\n  }\\n}\\n\",\"variables\":{\"id\":\"{Id}\"}}".Replace("{Id}", printable.Id.Replace("_Printables", string.Empty)), null, "application/json");
                                    requestPack.Content = contentPack;
                                    var responsePack = await client.SendAsync(requestPack);
                                    responsePack.EnsureSuccessStatusCode();
                                    var responseContentPack = await responsePack.Content.ReadAsStringAsync();

                                    // Parse the JSON response using JObject
                                    var json = JObject.Parse(responseContentPack);
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
                                    if (!string.IsNullOrEmpty(downloadLink)) 
                                    {
                                        var updatedFiles = new List<WebScrapperFunctionApp.Dto.File>();
                                        updatedFiles.AddRange(await BlobSerivce.UploadZipContent(printable.Id, downloadLink, "stl"));
                                        printable.PrintableDetials.Zip_data.Files = updatedFiles;

                                        var Images = printable.PrintableDetials.Zip_data.Images.ToList();
                                        var updatedImages = new List<WebScrapperFunctionApp.Dto.Image>();

                                        foreach (var item in Images)
                                        {
                                            updatedImages.Add(new WebScrapperFunctionApp.Dto.Image { name = item.name, url = await BlobSerivce.UploadFile(item.url, $"{printable.Id}_{Guid.NewGuid()}_{Guid.NewGuid()}{Path.GetExtension(item.url)}", "images") });
                                        }

                                        // Update the original collection after iteration
                                        printable.PrintableDetials.Zip_data.Images = updatedImages;
                                    }
                                }
                                if (printable.PrintableDetials.Zip_data.Files.Count>0)
                                {
                                    await elasticsearchService.UpsertDocument(printable, printable.Id).ConfigureAwait(false);
                                }
                                
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine($"Printable Detial Exception {ex}");
                SentrySdk.CaptureException(ex);

            }
            return new OkObjectResult($"TimerTrigger - PrintableDetialFunction Finished {DateTime.Now}");
        }
    }
}
