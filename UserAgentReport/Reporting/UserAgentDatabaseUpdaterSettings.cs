using System;

namespace Knapcode.UserAgentReport.Reporting
{
    public class UserAgentDatabaseUpdaterSettings
    {
        public TimeSpan RefreshPeriod { get; set; }
        public string DatabasePath { get; set; }
        public string StatusPath { get; set; }
        public Uri DatabaseUri { get; set; }
    }
}