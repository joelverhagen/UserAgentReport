using System;

namespace Knapcode.UserAgentReport.AccessLogs
{
    public class InvalidAccessLogLineException : ArgumentException
    {
        public InvalidAccessLogLineException(string message) : base(message)
        {
        }
    }
}