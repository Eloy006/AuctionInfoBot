using System;

namespace AuctionInfoBot
{
    public class Options
    {
        public static readonly string SectionKey = "Options";

        public ProxyOptions Proxy { get; set; }

        public string Token { get; set; }
    }

    public class ProxyOptions
    {
        public bool UseProxy { get; set; }
        public Uri Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}