using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Knapcode.UserAgentReport.WebApi.BusinessLogic
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

        public UserAgentDatabase(string databasePath)
        {
            _databasePath = databasePath;
        }

        public IEnumerable<UserAgentAndCount> GetTopUserAgents(int limit, bool includeBots, bool includeBrowsers)
        {
            using (var connection = GetConnection())
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