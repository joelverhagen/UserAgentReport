using System;

namespace Knapcode.UserAgentReport.WebApi.BusinessLogic
{
    public class UserAgentAndCount
    {
        public string UserAgent { get; set; }
        public UserAgentType Type { get; set; }
        public int Count { get; set; }
        public DateTimeOffset FirstSeen { get; set; }
        public DateTimeOffset LastSeen { get; set; }
    }
}