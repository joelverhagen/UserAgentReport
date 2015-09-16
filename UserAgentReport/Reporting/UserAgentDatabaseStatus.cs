using System;

namespace Knapcode.UserAgentReport.Reporting
{
    public class UserAgentDatabaseStatus
    {
        public DateTimeOffset? LastUpdated { get; set; }
        public UserAgentDatabaseStatusType Type { get; set; }
    }
}