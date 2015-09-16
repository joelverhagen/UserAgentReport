using System;
using System.IO;
using System.Web;
using Knapcode.UserAgentReport.AccessLogs;
using Knapcode.UserAgentReport.Reporting;

namespace Knapcode.UserAgentReport.WebApi.BusinessLogic
{
    public static class Singletons
    {
        private static readonly string DatabasePath = HttpContext.Current.Server.MapPath("~/App_Data/user-agents.sqlite3");

        private static readonly UserAgentDatabaseUpdaterSettings UserAgentDatabaseUpdaterSettings = new UserAgentDatabaseUpdaterSettings
        {
            RefreshPeriod = TimeSpan.FromHours(6),
            TemporaryDatabasePath = HttpContext.Current.Server.MapPath("~/App_Data/user-agents-latest.sqlite3"),
            DatabasePath = DatabasePath,
            StatusPath = HttpContext.Current.Server.MapPath("~/App_Data/user-agent-database-status.json"),
            DatabaseUri = new Uri("http://prancer.knapcode.com/data/user-agents.sqlite3")
        };

        private static readonly Lazy<UserAgentDatabaseUpdater> LazyUserAgentDatabaseUpdater = new Lazy<UserAgentDatabaseUpdater>(() => new UserAgentDatabaseUpdater(UserAgentDatabaseUpdaterSettings));
        private static readonly Lazy<UserAgentDatabase> LazyUserAgentDatabase = new Lazy<UserAgentDatabase>(() => new UserAgentDatabase(DatabasePath, TextWriter.Null, new CustomAccessLogParser()));

        public static UserAgentDatabaseUpdater UserAgentDatabaseUpdater
        {
            get { return LazyUserAgentDatabaseUpdater.Value; }
        }

        public static UserAgentDatabase UserAgentDatabase
        {
            get { return LazyUserAgentDatabase.Value; }
        }
    }
}