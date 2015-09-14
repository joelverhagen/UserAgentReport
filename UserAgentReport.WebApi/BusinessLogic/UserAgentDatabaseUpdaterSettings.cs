using System;

namespace Knapcode.UserAgentReport.WebApi.BusinessLogic
{
    public class UserAgentDatabaseUpdaterSettings
    {
        public TimeSpan RefreshPeriod { get; set; }
        public string TemporaryDatabasePath { get; set; }
        public string DatabasePath { get; set; }
        public string StatusPath { get; set; }
        public Uri DatabaseUri { get; set; }
    }
}