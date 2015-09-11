using System;
using System.Globalization;
using System.Text;

namespace Knapcode.UserAgentReport.AccessLogs
{
    public class CustomAccessLogParser : IAccessLogParser
    {
        public AccessLogEntry ParseVirtualHostCombinedFormat(string line)
        {
            var output = new AccessLogEntry();
            var shuttle = new StringShuttle(line);

            output.ServerName = shuttle.ReadToNext(':');
            shuttle.Read(1); // ':'

            output.ServerPort = int.Parse(shuttle.ReadToNext(' '));
            shuttle.Read(1); // ' '

            return ReadCombinedFormat(output, shuttle);
        }

        public AccessLogEntry ParseCombinedFormat(string line)
        {
            var output = new AccessLogEntry();
            var shuttle = new StringShuttle(line);

            return ReadCombinedFormat(output, shuttle);
        }

        private static AccessLogEntry ReadCombinedFormat(AccessLogEntry output, StringShuttle shuttle)
        {
            output.RemoteHostname = shuttle.ReadToNext(' ');
            shuttle.Read(1); // ' '

            output.RemoteLogname = shuttle.ReadToNext(' ');
            shuttle.Read(1); // ' '

            output.RemoteUser = shuttle.ReadToNext(' ');
            shuttle.Read(2); // ' ', '['

            output.Time = DateTimeOffset.ParseExact(shuttle.ReadToNext(']'), "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture);
            shuttle.Read(2); // ']', ' '

            output.Request = shuttle.ReadEscapedString();
            shuttle.Read(1); // ' '

            output.Status = int.Parse(shuttle.ReadToNext(' '));
            shuttle.Read(1); // ' '

            output.BytesSent = int.Parse(shuttle.ReadToNext(' '));
            shuttle.Read(1); // ' '

            output.Referer = shuttle.ReadEscapedString();
            shuttle.Read(1); // ' '

            output.UserAgent = shuttle.ReadEscapedString();

            return output;
        }

        public AccessLogEntry ParseLine(string line)
        {
            int colonIndex = line.IndexOf(':');
            int spaceIndex = line.IndexOf(' ');
            bool isVirtualHostFormat = colonIndex >= 0 && colonIndex < spaceIndex;
            return isVirtualHostFormat ? ParseVirtualHostCombinedFormat(line) : ParseCombinedFormat(line);
        }


        private class StringShuttle
        {
            private readonly string _input;
            private int _index;

            public string Remaining
            {
                get { return _input.Substring(_index); }
            }

            public char Current
            {
                get { return _input[_index]; }
            }

            public StringShuttle(string input)
            {
                _input = input;
                _index = 0;
            }

            public string Read(int count)
            {
                string output = _input.Substring(_index, count);
                _index += count;
                return output;
            }

            public string ReadToNext(char c)
            {
                var sb = new StringBuilder();
                while (_index < _input.Length && Current != c)
                {
                    sb.Append(Current);
                    _index++;
                }

                return sb.ToString();
            }

            public string ReadEscapedString()
            {
                if (Current != '"')
                {
                    string message = string.Format(
                        "The index {0} is pointing to a '{1}'. A '\"' is the expected beginning of an escaped string.",
                        _index,
                        Current);
                    throw new InvalidAccessLogLineException(message);
                }

                _index++;

                var sb = new StringBuilder();
                bool foundLastQuote = false;
                while (!foundLastQuote)
                {
                    if (Current == '"')
                    {
                        foundLastQuote = true;
                        _index++;
                    }
                    else if (Current == '\\')
                    {
                        sb.Append(ReadEscapedCharacter());
                    }
                    else
                    {
                        sb.Append(Read(1));
                    }
                }

                return sb.ToString();
            }

            /// <summary>
            /// Source: https://github.com/apache/httpd/blob/2fb2dff3c3019bf50fee9d61ae3e756193c31ab8/server/util.c#l2035-2058
            /// </summary>
            /// <returns></returns>
            private char ReadEscapedCharacter()
            {
                char output;
                switch (_input[_index + 1])
                {
                    case 'b':
                        output = '\b';
                        break;
                    case 'n':
                        output = '\n';
                        break;
                    case 'r':
                        output = '\r';
                        break;
                    case 't':
                        output = '\t';
                        break;
                    case 'v':
                        output = '\v';
                        break;
                    case '\\':
                        output = '\\';
                        break;
                    case '"':
                        output = '"';
                        break;
                    case 'x':
                        output = Convert.ToChar(Convert.ToByte(_input.Substring(_index + 2, 2), 16));
                        _index += 2;
                        break;
                    default:
                        string message = string.Format(
                            "An invalid escape sequence '{0}' was found at index {1}.",
                            _input.Substring(_index, 2),
                            _index);
                        throw new InvalidAccessLogLineException(message);
                }

                _index += 2;
                return output;
            }
        }
    }
}