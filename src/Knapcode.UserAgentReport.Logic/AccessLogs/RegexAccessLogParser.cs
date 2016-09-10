using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Knapcode.UserAgentReport.AccessLogs
{
    public class RegexAccessLogParser : IAccessLogParser
    {
        private const string EscapedString = @"[^""\\]*(?:\\.[^""\\]*)*";

        private const string CombinedFormat =
            @"(?<RemoteHostname>[^ ]+)" +
            " " +
            @"(?<RemoteLogname>[^ ]+)" +
            " " +
            @"(?<RemoteUser>[^ ]+)" +
            " " +
            @"(?<Time>\[[^\]]+\])" +
            " " +
            @"""(?<Request>" + EscapedString + @")""" +
            " " +
            @"(?<Status>\d+)" +
            " " +
            @"(?<BytesSent>\d+)" +
            " " +
            @"""(?<Referer>" + EscapedString + @")""" +
            " " +
            @"""(?<UserAgent>" + EscapedString + @")""";

        private const string VirtualHostCombinedFormat =
            @"(?<ServerName>[^:]+)" +
            ":" +
            @"(?<ServerPort>\d+)" +
            " " +
            CombinedFormat;

        public AccessLogEntry ParseLine(string line)
        {
            try
            {
                return ParseVirtualHostCombinedFormat(line);
            }
            catch (InvalidAccessLogLineException)
            {
            }

            return ParseCombinedFormat(line);
        }

        public AccessLogEntry ParseCombinedFormat(string line)
        {
            Match match = GetMatch(line, CombinedFormat, "combined");
            return InitializeFromCombinedMatch(match);
        }

        public AccessLogEntry ParseVirtualHostCombinedFormat(string line)
        {
            Match match = GetMatch(line, VirtualHostCombinedFormat, "virtual host combined");
            
            // parse the port
            int serverPort = int.Parse(match.Groups["ServerPort"].Value);

            var output = InitializeFromCombinedMatch(match);
            output.ServerName = match.Groups["ServerName"].Value;
            output.ServerPort = serverPort;

            return output;
        }

        private Match GetMatch(string line, string format, string formatName)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            Match match = Regex.Match(line, format, RegexOptions.Compiled);

            if (!match.Success)
            {
                throw new InvalidAccessLogLineException(string.Format("The line '{0}' could not be parsed using the {1} format.", line, formatName));
            }

            return match;
        }

        private AccessLogEntry InitializeFromCombinedMatch(Match match)
        {
            // parse the time
            DateTimeOffset time = DateTimeOffset.ParseExact(match.Groups["Time"].Value, "[dd/MMM/yyyy:HH:mm:ss zzz]", CultureInfo.InvariantCulture);

            // parse the status
            int status = int.Parse(match.Groups["Status"].Value);

            // parse the bytes sent
            int bytesSent = int.Parse(match.Groups["BytesSent"].Value);

            return new AccessLogEntry
            {
                RemoteHostname = match.Groups["RemoteHostname"].Value,
                RemoteLogname = match.Groups["RemoteLogname"].Value,
                RemoteUser = match.Groups["RemoteUser"].Value,
                Time = time,
                Request = match.Groups["Request"].Value,
                Status = status,
                BytesSent = bytesSent,
                Referer = match.Groups["Referer"].Value,
                UserAgent = match.Groups["UserAgent"].Value
            };
        }
    }
}