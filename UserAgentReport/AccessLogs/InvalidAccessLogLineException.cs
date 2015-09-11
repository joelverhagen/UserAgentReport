using System;
using System.Runtime.Serialization;

namespace Knapcode.UserAgentReport.AccessLogs
{
    [Serializable]
    public class InvalidAccessLogLineException : ArgumentException
    {
        public InvalidAccessLogLineException(string message) : base(message)
        {
        }

        protected InvalidAccessLogLineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}