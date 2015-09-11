using System;

namespace Knapcode.UserAgentReport.AccessLogs
{
    public class AccessLogEntry
    {
        public string ServerName { get; set; }
        public int? ServerPort { get; set; }
        public string RemoteHostname { get; set; }
        public string RemoteLogname { get; set; }
        public string RemoteUser { get; set; }
        public DateTimeOffset? Time { get; set; }
        public string Request { get; set; }
        public int? Status { get; set; }
        public int? BytesSent { get; set; }
        public string Referer { get; set; }
        public string UserAgent { get; set; }
    }
}