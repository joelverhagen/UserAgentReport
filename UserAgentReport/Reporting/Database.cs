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
    public class Database
    {
        private readonly string _databasePath;
        private readonly TextWriter _logWriter;
        private readonly IAccessLogParser _parser;

        public Database(string databasePath, TextWriter logWriter, IAccessLogParser parser)
        {
            _databasePath = databasePath;
            _logWriter = logWriter;
            _parser = parser;
        }

        public void Populate(string logDirectory, string logPattern)
        {
            using (var connection = GetConnection())
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

        public IEnumerable<UserAgentCount> Query()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT
                            *
                        FROM (
                            SELECT
                                MIN(ua.user_agent) AS user_agent,
                                MIN(uat.date_time) AS minimum_date_time,
                                MAX(uat.date_time) AS maximum_date_time,
                                COUNT(*) AS [count]
                            FROM user_agents ua
                            INNER JOIN user_agent_times uat
                            ON ua.id = uat.user_agent_id
                            GROUP BY ua.id
                        ) u
                        ORDER BY [count] DESC, maximum_date_time DESC";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new UserAgentCount
                            {
                                UserAgent = reader.GetString(0),
                                MinimumDateTime = new DateTimeOffset(reader.GetInt64(1), TimeSpan.Zero),
                                MaximumDateTime = new DateTimeOffset(reader.GetInt64(2), TimeSpan.Zero),
                                Count = reader.GetInt32(3)
                            };
                        }
                    }
                }
            }
        }

        private SQLiteConnection GetConnection()
        {
            var builder = new SQLiteConnectionStringBuilder { DataSource = _databasePath };
            var connection = new SQLiteConnection(builder.ConnectionString);
            return connection;
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
    }
}