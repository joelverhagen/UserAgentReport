using System;

namespace Knapcode.UserAgentReport.Reporting
{
    public class TopUserAgent
    {
        public string UserAgent { get; set; }
        public UserAgentType Type { get; set; }
        public int Count { get; set; }
        public DateTimeOffset FirstSeen { get; set; }
        public DateTimeOffset LastSeen { get; set; }
    }
}