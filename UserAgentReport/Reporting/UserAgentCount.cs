using System;

namespace Knapcode.UserAgentReport.Reporting
{
    public class UserAgentCount
    {
        public string UserAgent { get; set; }
        public DateTimeOffset MinimumDateTime { get; set; }
        public DateTimeOffset MaximumDateTime { get; set; }
        public int Count { get; set; }
    }
}
