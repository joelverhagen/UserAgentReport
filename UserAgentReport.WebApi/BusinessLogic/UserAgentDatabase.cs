using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Knapcode.UserAgentReport.WebApi.BusinessLogic
{
    public class UserAgentDatabase
    {
        private static readonly string[] NonBrowserKeywords =
        {
            "bot",
            "spider",
            "crawl",
            "slurp",
            "Feedfetcher-Google",
            "CloudFlare-AlwaysOnline"
        };

        private readonly string _databasePath;

        public UserAgentDatabase(string databasePath)
        {
            _databasePath = databasePath;
        }

        public IEnumerable<UserAgentAndCount> GetTopUserAgents(int limit, bool onlyBrowsers)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    string whereClause = string.Empty;
                    if (onlyBrowsers)
                    {
                        var conditions = new List<string> {"ua.user_agent != '-'"};
                        foreach (var keyword in NonBrowserKeywords)
                        {
                            string parameterName = "@exclude" + conditions.Count;
                            command.Parameters.Add(parameterName, DbType.String).Value = keyword;
                            conditions.Add("ua.user_agent NOT LIKE '%' || " + parameterName + " || '%'");
                        }

                        whereClause = " WHERE " + string.Join(" AND ", conditions);
                    }

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
                            " + whereClause + @"
                            GROUP BY ua.id
                        ) u
                        ORDER BY [count] DESC, maximum_date_time DESC
                        LIMIT @limit";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@limit", DbType.Int32).Value = limit;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new UserAgentAndCount
                            {
                                UserAgent = reader.GetString(0),
                                Count = reader.GetInt32(3)
                            };
                        }
                    }
                }
            }
        }

        private SQLiteConnection GetConnection()
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = _databasePath,
                ReadOnly = true
            };

            return new SQLiteConnection(builder.ConnectionString);
        }
    }
}