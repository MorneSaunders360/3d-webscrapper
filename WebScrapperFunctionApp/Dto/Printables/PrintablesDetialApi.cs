using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapperFunctionApp.Dto.PrintablesDetial
{

    public class Data
    {
        [JsonProperty("print")]
        public Print Print { get; set; }
    }



    public class Image
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("filePath")]
        public string FilePath { get; set; }

    }


    public class Print
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("stls")]
        public List<Stl> Stls { get; set; }

    }


    public class PrintablesDetialApi
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Stl
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }


        [JsonProperty("filePreviewPath")]
        public string FilePreviewPath { get; set; }

    }



    public class User
    {
        [JsonProperty("publicUsername")]
        public string PublicUsername { get; set; }


        [JsonProperty("handle")]
        public string Handle { get; set; }



    }


}
