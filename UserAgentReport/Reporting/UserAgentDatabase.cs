using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Text;
using Knapcode.UserAgentReport.AccessLogs;

namespace Knapcode.UserAgentReport.Reporting
{
    public class UserAgentDatabase
    {
        private static readonly string[] BotKeywords =
        {
            "%bot%",
            "%spider%",
            "%crawl%",
            "%slurp%",
            "%Feedfetcher-Google%",
            "%CloudFlare-AlwaysOnline%",
            "%Transmission/%",
            "%Lipperhey%",
            "%InfoPath%",
            "%WPDesktop%",
            "%coccoc%",
            "%Netcraft SSL Server Survey%",
            "%WordPress.com mShots%",
            "%baidu.com%",
            "%LinkWalker%",
            "%TheFreeDictionary.com%",
            "%WebMasterAid%",
            "%facebookexternalhit%",
            "%moz.com%",
            "%Ezooms%",
            "%panscient.com%",
            "%StumbleUpon%",
            "%TencentTraveler%",
            "%GoogleImageProxy%",
            "%Google favicon%",
            "%WinHttpRequest%",
            "WinInet Test",
            "Apache/%",
            "googleweblight",
            "%Daumoa%",
            "%SiteExplorer/%",
            "Java/%",
            "python-%",
            "go _._ package http",
            "nutch-%",
            "Ruby",
            "%com.apple.WebKit.WebContent%",
            "%Google Web Preview%",
            "curl/%",
            "PiranhaLite",
            "%Blog Search;%",
            "%DigExt%",
            "-"
        };

        private readonly string _databasePath;
        private readonly TextWriter _logWriter;
        private readonly IAccessLogParser _parser;

        public UserAgentDatabase(string databasePath, TextWriter logWriter, IAccessLogParser parser)
        {
            _databasePath = databasePath;
            _logWriter = logWriter;
            _parser = parser;
        }

        public IEnumerable<UserAgentAndCount> GetTopUserAgents(int limit, bool includeBots, bool includeBrowsers)
        {
            using (var connection = GetConnection(true))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // add parameters
                    command.Parameters.Add("@limit", DbType.Int32).Value = limit;
                    command.Parameters.Add("@browser", DbType.Int32).Value = UserAgentType.Browser;
                    command.Parameters.Add("@bot", DbType.Int32).Value = UserAgentType.Bot;

                    // determine if a user agent is a bot
                    var isBotConditions = new List<string>();
                    foreach (var botKeyword in BotKeywords)
                    {
                        var parameterName = "@botKeyword" + isBotConditions.Count;
                        command.Parameters.Add(parameterName, DbType.String).Value = botKeyword;
                        isBotConditions.Add("user_agent LIKE " + parameterName);
                    }

                    var isBotCondition = string.Join(" OR ", isBotConditions);

                    // determine if a user agent should be included
                    var includeConditions = new List<string>();
                    if (!includeBots)
                    {
                        includeConditions.Add("type != @bot");
                    }

                    if (!includeBrowsers)
                    {
                        includeConditions.Add("type != @browser");
                    }

                    var includeCondition = includeConditions.Count > 0 ? "WHERE " + string.Join(" AND ", includeConditions) : string.Empty;

                    // build the query
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"DROP TABLE IF EXISTS _matched_user_agents;
CREATE TEMP TABLE _matched_user_agents (id INTEGER, user_agent TEXT, type INTEGER);

INSERT INTO _matched_user_agents (id, user_agent, type)
SELECT
    id,
    user_agent,
    type
FROM (
    SELECT
        id,
        user_agent,
        CASE WHEN " + isBotCondition + @" THEN @bot ELSE @browser END AS type
    FROM user_agents
)
" + includeCondition + @";

SELECT
    user_agent,
    type,
    [count],
    first_seen,
    last_seen
FROM (
    SELECT
        MIN(ua.user_agent) AS user_agent,
        ua.type AS type,
        COUNT(*) AS [count],
        MIN(uat.date_time) AS first_seen,
        MAX(uat.date_time) AS last_seen
    FROM _matched_user_agents ua
    INNER JOIN user_agent_times uat
    ON ua.id = uat.user_agent_id
    GROUP BY ua.id
)
ORDER BY [count] DESC, last_seen DESC
LIMIT @limit;

DROP TABLE IF EXISTS _matched_user_agents;
";
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new UserAgentAndCount
                            {
                                UserAgent = reader.GetString(0),
                                Type = (UserAgentType) reader.GetInt32(1),
                                Count = reader.GetInt32(2),
                                FirstSeen = new DateTimeOffset(reader.GetInt64(3), TimeSpan.Zero),
                                LastSeen = new DateTimeOffset(reader.GetInt64(4), TimeSpan.Zero)
                            };
                        }
                    }
                }
            }
        }

        public void Populate(string logDirectory, string logPattern)
        {
            using (var connection = GetConnection(false))
            {
                connection.Open();
                InitializeLatestTables(connection);

                foreach (var filePath in Directory.EnumerateFiles(logDirectory, logPattern, SearchOption.TopDirectoryOnly))
                {
                    _logWriter.WriteLine("Parsing {0}...", filePath);
                    PersistAccessLogFile(connection, filePath);
                }

                SwapLatestTables(connection);
                Vacuum(connection);
            }
        }

        private void Vacuum(SQLiteConnection connection)
        {
            Execute(connection, "VACUUM");
        }

        private void InitializeLatestTables(SQLiteConnection connection)
        {
            Execute(connection, "DROP TABLE IF EXISTS user_agents_latest; " +
                                "DROP TABLE IF EXISTS user_agent_times_latest; " +
                                "CREATE TABLE user_agents_latest (id INTEGER PRIMARY KEY, user_agent TEXT UNIQUE); " +
                                "CREATE TABLE user_agent_times_latest (user_agent_id INTEGER, date_time INTEGER);");
        }

        private void SwapLatestTables(SQLiteConnection connection)
        {
            Execute(connection, "DROP TABLE IF EXISTS user_agents;" +
                                "DROP TABLE IF EXISTS user_agent_times; " +
                                "ALTER TABLE user_agents_latest RENAME TO user_agents; " +
                                "ALTER TABLE user_agent_times_latest RENAME TO user_agent_times;");
        }

        private void PersistEntry(SQLiteConnection connection, AccessLogEntry entry)
        {
            if (!entry.Time.HasValue)
            {
                return;
            }

            // get the user agent ID
            long id;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id FROM user_agents_latest WHERE user_agent = @user_agent";
                command.Parameters.Add("@user_agent", DbType.String).Value = entry.UserAgent;
                using (var commandReader = command.ExecuteReader())
                {
                    if (commandReader.Read())
                    {
                        id = commandReader.GetInt64(0);
                    }
                    else
                    {
                        id = -1;
                    }
                }
            }

            // persist the user agent if it does not exist yet
            if (id == -1)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO user_agents_latest (user_agent) VALUES (@user_agent); SELECT last_insert_rowid();";
                    command.Parameters.Add("@user_agent", DbType.String).Value = entry.UserAgent;
                    using (var commandReader = command.ExecuteReader())
                    {
                        commandReader.Read();
                        id = commandReader.GetInt64(0);
                    }
                }
            }

            // persiste the user agent time
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO user_agent_times_latest (user_agent_id, date_time) VALUES (@user_agent_id, @date_time)";
                command.Parameters.Add("@user_agent_id", DbType.Int64).Value = id;
                command.Parameters.Add("@date_time", DbType.Int64).Value = entry.Time.Value.UtcTicks;
                command.ExecuteNonQuery();
            }
        }

        private void PersistAccessLogFile(SQLiteConnection connection, string filePath)
        {
            Execute(connection, "BEGIN");

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // decompress, if necessary
                Stream readStream = fileStream;
                if (filePath.EndsWith(".gz"))
                {
                    readStream = new GZipStream(fileStream, CompressionMode.Decompress);
                }

                // parse the file, line by line
                using (var streamReader = new StreamReader(readStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var entry = _parser.ParseLine(line);
                        PersistEntry(connection, entry);
                    }
                }
            }

            Execute(connection, "COMMIT");
        }

        private static void Execute(SQLiteConnection connection, string sql)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        private SQLiteConnection GetConnection(bool readOnly)
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = _databasePath,
                ReadOnly = readOnly
            };

            return new SQLiteConnection(builder.ConnectionString);
        }
    }
}