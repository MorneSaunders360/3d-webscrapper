using System;
using System.Diagnostics.Metrics;
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

namespace WebScrapperFunctionApp
{
    public class PrintableDetialFunction
    {

        [Function("PrintableDetialFunction")]
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
                                                             .Term(t => t
                                                                 .Field(f => f.PrintableDetials)
                                                                 .Value(null)
                                                             )
                                                         )
                                                     )
                                                 )
                                             );
                int Counter = 0;
                var tasks = searchResponseprintable.Documents.Select(async printable =>
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
                            string ThumbnailLink = await BlobSerivce.UploadFile("https://files.printables.com/" + PrintablesDetialApi.Data.Print.Image.FilePath, $"{Guid.NewGuid()}_{Guid.NewGuid()}.{Path.GetExtension(PrintablesDetialApi.Data.Print.Image.FilePath)}", "images");
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
                            var files = PrintablesDetialApi.Data.Print.Stls.ToList();
                            await elasticsearchService.UpsertDocument(printable, printable.Id).ConfigureAwait(false);
                            if (files.Count > 0)
                            {
                                printable.PrintableDetials.Zip_data = new ZipData() { Files = new List<WebScrapperFunctionApp.Dto.File>(), Images = new List<WebScrapperFunctionApp.Dto.Image>() };
                                foreach (var item in files)
                                {
                                    var clientFile = new HttpClient();
                                    var requestFile = new HttpRequestMessage(HttpMethod.Post, "https://api.printables.com/graphql/");
                                    requestFile.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Edg/126.0.0.0");
                                    var contentFile = new StringContent("{\"query\":\"mutation GetDownloadLink($id: ID!, $printId: ID!, $fileType: DownloadFileTypeEnum!, $source: DownloadSourceEnum!) {\\n  getDownloadLink(\\n    id: $id\\n    printId: $printId\\n    fileType: $fileType\\n    source: $source\\n  ) {\\n    ok\\n    errors {\\n      field\\n      messages\\n      __typename\\n    }\\n    output {\\n      link\\n      count\\n      ttl\\n      __typename\\n    }\\n    __typename\\n  }\\n}\",\"variables\":{\"id\":\"{FileId}\",\"fileType\":\"stl\",\"printId\":\"{PrintId}\",\"source\":\"model_detail\"}}".Replace("{FileId}", item.Id).Replace("{PrintId}", PrintablesDetialApi.Data.Print.Id), null, "application/json");
                                    requestFile.Content = contentFile;
                                    var responseFile = await clientFile.SendAsync(requestFile);
                                    if (responseFile.IsSuccessStatusCode)
                                    {
                                        JObject jsonObject = JObject.Parse(await responseFile.Content.ReadAsStringAsync());
                                        string link = await BlobSerivce.UploadFile(jsonObject["data"]["getDownloadLink"]["output"]["link"].ToString(), $"{Guid.NewGuid()}_{Guid.NewGuid()}.{Path.GetExtension(jsonObject["data"]["getDownloadLink"]["output"]["link"].ToString())}", "stl");
                                        printable.PrintableDetials.Zip_data.Files.Add(new WebScrapperFunctionApp.Dto.File { url = link, name = item.Name });
                                    }

                                    printable.PrintableDetials.Zip_data.Images.Add(new WebScrapperFunctionApp.Dto.Image { name = item.Name, url = await BlobSerivce.UploadFile("https://files.printables.com/" + item.FilePreviewPath, $"{Guid.NewGuid()}_{Guid.NewGuid()}.{Path.GetExtension(item.FilePreviewPath)}", "images") });
                                }


                            }
                            SentrySdk.CaptureMessage($"Printable Detial Uploaded {Counter++} {printable?.Id}");
                            Console.WriteLine($"Printable Detial Uploaded {Counter++}");
                            await elasticsearchService.UpsertDocument(printable, printable.Id).ConfigureAwait(false);
                        }

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
