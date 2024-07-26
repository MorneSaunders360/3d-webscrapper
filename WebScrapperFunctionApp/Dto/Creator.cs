using Newtonsoft.Json;

namespace WebScrapperFunctionApp.Dto
{
    public class Creator
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }

    }
}
