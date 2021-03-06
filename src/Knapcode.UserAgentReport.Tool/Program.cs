﻿using System;
using System.IO;
using System.Reflection;
using Knapcode.UserAgentReport.AccessLogs;
using Knapcode.UserAgentReport.Reporting;
using Microsoft.Extensions.Options;

namespace Knapcode.UserAgentReport.Tool
{
    public class Program
    {
        private static int Main(string[] args)
        {
            var applicationName = Assembly.GetEntryAssembly().GetName().Name;
            var baseDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (args.HasOption("help") || args.HasOption("h"))
            {
                Console.Error.WriteLine($"Usage: {applicationName} OPTIONS");
                Console.Error.WriteLine();
                Console.Error.WriteLine("OPTIONS can be any combination of the following:");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  -help, -h, -?   Show this help message.");
                Console.Error.WriteLine();
                Console.Error.WriteLine("  -populate       Read the latest access logs and populate the user agent database.");
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
            var databasePath = args.GetOption("db", Path.Combine(baseDirectory, "user_agents.sqlite3"));

            // initialize
            var database = new UserAgentDatabase(
                Options.Create(new UserAgentDatabaseUpdaterSettings
                {
                    DatabasePath = databasePath
                }),
                Console.Error,
                new CustomAccessLogParser());

            // populate
            if (args.HasOption("populate"))
            {
                string logPattern = Path.GetFileName(logDirectory);
                logDirectory = Path.GetDirectoryName(logDirectory);
                database.Populate(logDirectory, logPattern);
            }

            return 0;
        }
    }
}