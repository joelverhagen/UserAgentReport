namespace Knapcode.UserAgentReport.AccessLogs
{
    public interface IAccessLogParser
    {
        AccessLogEntry ParseLine(string line);
        AccessLogEntry ParseCombinedFormat(string line);
        AccessLogEntry ParseVirtualHostCombinedFormat(string line);
    }
}