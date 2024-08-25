using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapperFunctionApp.Dto
{
    public class IpData
    {
        [JsonProperty("as")]
        public string As { get; set; }

        [JsonProperty("asname")]
        public string Asname { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("continent")]
        public string Continent { get; set; }

        [JsonProperty("continentCode")]
        public string ContinentCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("hosting")]
        public bool Hosting { get; set; }

        [JsonProperty("isp")]
        public string Isp { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("mobile")]
        public bool Mobile { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("proxy")]
        public bool Proxy { get; set; }

        [JsonProperty("regionName")]
        public string RegionName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }
    }

    public class Proxy
    {

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("ssl")]
        public bool Ssl { get; set; }

        public string Ip { get; set; }
    }

    public class NewProxy
    {

        [JsonProperty("proxies")]
        public List<Proxy> Proxies { get; set; }
    }


}
