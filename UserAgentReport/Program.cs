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
            if (args.HasOption("help") || args.HasOption("h"))
            {
                Console.Error.WriteLine("Usage: {0} OPTIONS", AppDomain.CurrentDomain.FriendlyName);
                Console.Error.WriteLine();
                Console.Error.WriteLine("OPTIONS can be any combination of the following:");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  -help, -h, -?   Show this help message.");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  -populate       Read the latest access logs and populate the user agent database.");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  -query          Query the user agent database for the user agents and dump the results to STDOUT.");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  -pretty         Output the results of the query in indented JSON.");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  -db PATH        Read and write to the database at the provided path.");
                Console.Error.WriteLine("                  Default: 'user_agents.sqlite3' in the application directory.");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  -logs PATTERN   Read Apache access logs matching the provided pattern.");
                Console.Error.WriteLine("                  Default: '/var/log/apache2/access*'");
                return 1;
            }

            // arguments
            var logDirectory = args.GetOption("logs", "/var/log/apache2/access*");
            var databasePath = args.GetOption("db", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_agents.sqlite3"));

            // initialize
            var database = new Database(databasePath, Console.Error, new CustomAccessLogParser());

            // populate
            if (args.HasOption("populate"))
            {
                string logPattern = Path.GetFileName(logDirectory);
                logDirectory = Path.GetDirectoryName(logDirectory);
                database.Populate(logDirectory, logPattern);
            }
            
            // query
            if (args.HasOption("query"))
            {
                var counts = database.Query().ToArray();
                Formatting formatting = args.HasOption("pretty") ? Formatting.Indented : Formatting.None;
                Console.WriteLine(JsonConvert.SerializeObject(counts, formatting));
            }
            
            return 0;
        }
    }
}