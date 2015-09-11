using System;
using System.IO;
using System.Linq;
using Knapcode.UserAgentReport.AccessLogs;
using Knapcode.UserAgentReport.Reporting;
using Newtonsoft.Json;

namespace Knapcode.UserAgentReport
{
    public class Program
    {
        private static int Main(string[] args)
        {
            // arguments
            var logDirectory = "/var/log/apache2";
            var logPattern = @"access*";
            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_agents.sqlite3");

            // initialize
            var database = new Database(databasePath, Console.Error, new CustomAccessLogParser());

            // populate
            database.Populate(logDirectory, logPattern);

            // query
            var counts = database.Query().ToArray();

            // output
            Console.WriteLine(JsonConvert.SerializeObject(counts, Formatting.Indented));
            
            return 0;
        }
    }
}