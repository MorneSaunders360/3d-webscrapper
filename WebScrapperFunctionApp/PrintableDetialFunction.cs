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
using WebScrapperFunctionApp.Dto.PrintablesDetial;
using static System.Net.WebRequestMethods;

namespace WebScrapperFunctionApp
{

    public class PrintableDetialFunction
    {


        [Function("PrintableDetialFunctionHttp")]
        public async Task<IActionResult> PrintableDetialFunctionHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
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
            List<string> userAgents = new List<string>
        {

            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",

        };

            // Generate a random index
            var random = new Random();
            int randomIndex = random.Next(userAgents.Count);
            // Select a random UA using the randomIndex
           

            // Set selected User Agent
            
            try
            {
                var elasticsearchService = new ElasticsearchService<Printable>("printables");
                int Counter = 0;
                Console.WriteLine($"Found Printable Detial For Processsing {dtRequest.Count()}");
                var tasks = dtRequest.Select(async printable =>
                {
                    try
                    {
                        
                        if (printable.Type.ToLower() == "printables" && (printable.PrintableDetials == null || printable.PrintableDetials.Zip_data == null || printable.PrintableDetials.Zip_data.Files.Count == 0))
                        {
                            var client = new HttpClient();
                            
                            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.printables.com/graphql/");
                            string randomUserAgent = userAgents[randomIndex];
                            request.Headers.Add("User-Agent", randomUserAgent);
                            var JsonContent = "{\"query\":\"query PrintProfile($id: ID!, $loadPurchase: Boolean!) {\\n  print(id: $id) {\\n    ...PrintDetailFragment\\n    price\\n    user {\\n      billingAccountType\\n      lowestTierPrice\\n      highlightedModels {\\n        models {\\n          ...PrintListFragment\\n          __typename\\n        }\\n        featured\\n        __typename\\n      }\\n      __typename\\n    }\\n    purchaseDate @include(if: $loadPurchase)\\n    paidPrice @include(if: $loadPurchase)\\n    __typename\\n  }\\n}\\n\\nfragment PrintDetailFragment on PrintType {\\n  id\\n  slug\\n  name\\n  authorship\\n  remixDescription\\n  premium\\n  price\\n  excludeCommercialUsage\\n  eduProject {\\n    id\\n    subject {\\n      id\\n      name\\n      slug\\n      __typename\\n    }\\n    language {\\n      id\\n      name\\n      __typename\\n    }\\n    free\\n    timeDifficulty\\n    audienceAge\\n    complexity\\n    equipment {\\n      id\\n      name\\n      __typename\\n    }\\n    suitablePrinters {\\n      id\\n      name\\n      __typename\\n    }\\n    organisation\\n    authors\\n    targetGroupFocus\\n    knowledgeAndSkills\\n    objectives\\n    equipmentDescription\\n    timeSchedule\\n    workflow\\n    approved\\n    datePublishRequested\\n    __typename\\n  }\\n  user {\\n    ...AvatarUserFragment\\n    isFollowedByMe\\n    canBeFollowed\\n    publishedPrintsCount\\n    premiumPrintsCount\\n    designer\\n    stripeAccountActive\\n    membership {\\n      currentTier {\\n        id\\n        name\\n        benefits {\\n          id\\n          title\\n          benefitType\\n          description\\n          __typename\\n        }\\n        __typename\\n      }\\n      __typename\\n    }\\n    __typename\\n  }\\n  ratingAvg\\n  myRating\\n  ratingCount\\n  description\\n  category {\\n    id\\n    path {\\n      id\\n      name\\n      nameEn\\n      storeName\\n      description\\n      storeDescription\\n      __typename\\n    }\\n    __typename\\n  }\\n  mmu\\n  modified\\n  firstPublish\\n  datePublished\\n  dateCreatedThingiverse\\n  nsfw\\n  summary\\n  likesCount\\n  makesCount\\n  liked\\n  printDuration\\n  numPieces\\n  weight\\n  nozzleDiameters\\n  usedMaterial\\n  layerHeights\\n  materials {\\n    id\\n    name\\n    __typename\\n  }\\n  dateFeatured\\n  downloadCount\\n  displayCount\\n  filesCount\\n  privateCollectionsCount\\n  publicCollectionsCount\\n  pdfFilePath\\n  commentCount\\n  userGcodeCount\\n  remixCount\\n  canBeRated\\n  printer {\\n    id\\n    name\\n    __typename\\n  }\\n  image {\\n    ...ImageSimpleFragment\\n    __typename\\n  }\\n  images {\\n    ...ImageSimpleFragment\\n    __typename\\n  }\\n  tags {\\n    name\\n    id\\n    __typename\\n  }\\n  thingiverseLink\\n  filesType\\n  license {\\n    id\\n    disallowRemixing\\n    __typename\\n  }\\n  remixParents {\\n    ...remixParentDetail\\n    __typename\\n  }\\n  gcodes {\\n    id\\n    name\\n    fileSize\\n    filePreviewPath\\n    __typename\\n  }\\n  stls {\\n    id\\n    name\\n    fileSize\\n    filePreviewPath\\n    __typename\\n  }\\n  slas {\\n    id\\n    name\\n    fileSize\\n    filePreviewPath\\n    __typename\\n  }\\n  ...LatestCompetitionResult\\n  competitions {\\n    id\\n    name\\n    slug\\n    description\\n    isOpen\\n    __typename\\n  }\\n  competitionResults {\\n    placement\\n    competition {\\n      id\\n      name\\n      slug\\n      printsCount\\n      openFrom\\n      openTo\\n      __typename\\n    }\\n    __typename\\n  }\\n  __typename\\n}\\n\\nfragment AvatarUserFragment on UserType {\\n  id\\n  publicUsername\\n  avatarFilePath\\n  handle\\n  company\\n  verified\\n  badgesProfileLevel {\\n    profileLevel\\n    __typename\\n  }\\n  __typename\\n}\\n\\nfragment ImageSimpleFragment on PrintImageType {\\n  id\\n  filePath\\n  rotation\\n  __typename\\n}\\n\\nfragment remixParentDetail on PrintRemixType {\\n  id\\n  parentPrintId\\n  parentPrintName\\n  parentPrintAuthor {\\n    id\\n    publicUsername\\n    verified\\n    handle\\n    __typename\\n  }\\n  parentPrint {\\n    id\\n    name\\n    slug\\n    datePublished\\n    image {\\n      ...ImageSimpleFragment\\n      __typename\\n    }\\n    premium\\n    authorship\\n    license {\\n      id\\n      name\\n      disallowRemixing\\n      __typename\\n    }\\n    eduProject {\\n      id\\n      __typename\\n    }\\n    __typename\\n  }\\n  url\\n  urlAuthor\\n  urlImage\\n  urlTitle\\n  urlLicense {\\n    id\\n    name\\n    disallowRemixing\\n    __typename\\n  }\\n  urlLicenseText\\n  __typename\\n}\\n\\nfragment LatestCompetitionResult on PrintType {\\n  latestCompetitionResult {\\n    placement\\n    competitionId\\n    __typename\\n  }\\n  __typename\\n}\\n\\nfragment PrintListFragment on PrintType {\\n  id\\n  name\\n  slug\\n  ratingAvg\\n  likesCount\\n  liked\\n  datePublished\\n  dateFeatured\\n  firstPublish\\n  downloadCount\\n  category {\\n    id\\n    path {\\n      id\\n      name\\n      __typename\\n    }\\n    __typename\\n  }\\n  modified\\n  image {\\n    ...ImageSimpleFragment\\n    __typename\\n  }\\n  nsfw\\n  premium\\n  price\\n  user {\\n    ...AvatarUserFragment\\n    __typename\\n  }\\n  ...LatestCompetitionResult\\n  __typename\\n}\",\"variables\":{\"id\":\"{Id}\",\"loadPurchase\":false}}";
                            var content = new StringContent(JsonContent.Replace("{Id}", printable.Id.Replace("_Printables", string.Empty)), null, "application/json");
                            request.Content = content;
                            var response = await client.SendAsync(request);
                            if (response.IsSuccessStatusCode)
                            {
                                PrintablesDetialApi PrintablesDetialApi = JsonConvert.DeserializeObject<PrintablesDetialApi>(await response.Content.ReadAsStringAsync());
                                if (PrintablesDetialApi.Data.Print!=null)
                                {
                                    string ThumbnailLink = "https://files.printables.com/" + PrintablesDetialApi.Data.Print.Image?.FilePath?? "https://www.threedprintingservices.com/splash.svg";
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
                                    var updatedFiles = new List<WebScrapperFunctionApp.Dto.File>();
                                    var tasksStl = PrintablesDetialApi.Data.Print.Stls.Select(async item =>
                                    {
                                        var clientSingleFile = new HttpClient();
                                        var requestSingleFile = new HttpRequestMessage(HttpMethod.Post, "https://api.printables.com/graphql/");
                                        string randomUserAgent = userAgents[randomIndex];
                                        requestSingleFile.Headers.Add("User-Agent", randomUserAgent);
                                        requestSingleFile.Headers.Add("Accept", "application/json, text/plain, */*");
                                        string Json = "{\"query\":\"mutation GetDownloadLink($id: ID!, $printId: ID!, $fileType: DownloadFileTypeEnum!, $source: DownloadSourceEnum!) {\\n  getDownloadLink(\\n    id: $id\\n    printId: $printId\\n    fileType: $fileType\\n    source: $source\\n  ) {\\n    ok\\n    errors {\\n      field\\n      messages\\n      __typename\\n    }\\n    output {\\n      link\\n      count\\n      ttl\\n      __typename\\n    }\\n    __typename\\n  }\\n}\",\"variables\":{\"id\":\"{fileId}\",\"fileType\":\"stl\",\"printId\":\"{printId}\",\"source\":\"model_detail\"}}".Replace("{fileId}", item.Id).Replace("{printId}", printable.Id.Replace("_Printables", string.Empty));
                                        var contentSingleFile = new StringContent(Json, null, "application/json");
                                        requestSingleFile.Content = contentSingleFile;
                                        var responseSingleFile = await client.SendAsync(requestSingleFile);
                                        if (!response.IsSuccessStatusCode)
                                        {
                                            Console.WriteLine(response.Content.ReadAsStringAsync());
                                            return;
                                        }
                                        var jsonDownloadPackLink = JObject.Parse(await responseSingleFile.Content.ReadAsStringAsync());
                                        var downloadLink = jsonDownloadPackLink["data"]?["getDownloadLink"]?["output"]?["link"]?.ToString();
                                        if (!string.IsNullOrEmpty(downloadLink))
                                        {
                                            updatedFiles.Add(new WebScrapperFunctionApp.Dto.File { name = item.Name, url = downloadLink });
                                        }
                                    }).ToList();
                                    var tasksGcodes = PrintablesDetialApi.Data.Print.Gcodes.Select(async item =>
                                    {
                                        var clientSingleFile = new HttpClient();
                                        var requestSingleFile = new HttpRequestMessage(HttpMethod.Post, "https://api.printables.com/graphql/");
                                        requestSingleFile.Headers.Add("Accept", "application/json, text/plain, */*");
                                        string randomUserAgent = userAgents[randomIndex];
                                        requestSingleFile.Headers.Add("User-Agent", randomUserAgent);
                                        string Json = "{\"query\":\"mutation GetDownloadLink($id: ID!, $printId: ID!, $fileType: DownloadFileTypeEnum!, $source: DownloadSourceEnum!) {\\n  getDownloadLink(\\n    id: $id\\n    printId: $printId\\n    fileType: $fileType\\n    source: $source\\n  ) {\\n    ok\\n    errors {\\n      field\\n      messages\\n      __typename\\n    }\\n    output {\\n      link\\n      count\\n      ttl\\n      __typename\\n    }\\n    __typename\\n  }\\n}\",\"variables\":{\"id\":\"{fileId}\",\"fileType\":\"{stl}\",\"printId\":\"{printId}\",\"source\":\"model_detail\"}}".Replace("{fileId}", item.Id).Replace("{printId}", printable.Id.Replace("_Printables", string.Empty)).Replace("{stl}", Path.GetExtension(item.Name).Replace(".", string.Empty).ToLower());
                                        var contentSingleFile = new StringContent(Json, null, "application/json");
                                        requestSingleFile.Content = contentSingleFile;
                                        var responseSingleFile = await client.SendAsync(requestSingleFile);
                                        if (!response.IsSuccessStatusCode)
                                        {
                                            Console.WriteLine(response.Content.ReadAsStringAsync());
                                            return;
                                        }
                                        var jsonDownloadPackLink = JObject.Parse(await responseSingleFile.Content.ReadAsStringAsync());
                                        var downloadLink = jsonDownloadPackLink["data"]?["getDownloadLink"]?["output"]?["link"]?.ToString();
                                        if (!string.IsNullOrEmpty(downloadLink))
                                        {
                                            updatedFiles.Add(new WebScrapperFunctionApp.Dto.File { name = item.Name, url = downloadLink });
                                        }
                                    }).ToList();
                                    await Task.WhenAll(tasksStl);
                                    await Task.WhenAll(tasksGcodes);
                                    printable.PrintableDetials.Zip_data.Files = updatedFiles;
                                    if (printable.PrintableDetials.Zip_data.Files.Count == 0)
                                    {
                                        return;
                                    }
                                    var Images = PrintablesDetialApi.Data.Print.Stls.ToList();
                                    var updatedImages = new List<WebScrapperFunctionApp.Dto.Image>();
                                    foreach (var item in Images)
                                    {
                                        updatedImages.Add(new WebScrapperFunctionApp.Dto.Image { name = item.Name, url = "https://files.printables.com/" + item.FilePreviewPath });
                                    }
                                    printable.PrintableDetials.Zip_data.Images = updatedImages;
                                    printable.CreatedDate = DateTime.Now;
                                    Counter++;
                                    Console.WriteLine($"Printable Detial Uploaded {Counter}");
                                    await elasticsearchService.UpsertDocument(printable, printable.Id).ConfigureAwait(false);
                                }
                              
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Printable Detial Exception {ex}");
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
            return new OkObjectResult($"TimerTrigger - PrintableDetialFunction Finished {DateTime.Now}");
        }
    }
}
