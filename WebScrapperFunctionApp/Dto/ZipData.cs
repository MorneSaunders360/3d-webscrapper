using Newtonsoft.Json;

namespace WebScrapperFunctionApp.Dto
{
    public class ZipData
    {
        [JsonProperty("files")]
        public List<File> Files { get; set; }
        [JsonProperty("images")]
        public List<Image> Images { get; set; }
    }
}
