using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapperFunctionApp.Dto
{
    internal class Printable
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public PrintableDetials PrintableDetials { get; set; }
    }
    internal class PrintableDetials
    {
        public string Name { get; set; }
        public double Volume { get; set; }
        public string Thumbnail { get; set; }
        public double Cost { get; set; }
        public string Url { get; set; }
        public string Public_url { get; set; }
        public Creator Creator { get; set; }
        public string License { get; set; }
        public ZipData Zip_data { get; set; }
    }
}
