namespace WebScrapperFunctionApp.Dto
{
    public class ProxyInfo
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public bool IsValid { get; set; }

        public long ResponseTime { get; set; }
    }

}
