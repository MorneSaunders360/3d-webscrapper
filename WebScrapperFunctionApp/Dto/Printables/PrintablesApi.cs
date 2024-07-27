using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapperFunctionApp.Dto.Printables
{


    public class Data
    {
        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    public class Image
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

    }

    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

    }


    public class Result
    {
        [JsonProperty("items")]
        public List<Item> Items { get; set; }

    }

    public class PrintablesApi
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }


    }


}
