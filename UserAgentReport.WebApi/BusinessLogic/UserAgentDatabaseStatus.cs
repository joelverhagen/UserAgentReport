using System;

namespace Knapcode.UserAgentReport.WebApi.BusinessLogic
{
    public class UserAgentDatabaseStatus
    {
        public DateTimeOffset? LastUpdated { get; set; }
        public UserAgentDatabaseStatusType Type { get; set; }
    }
}