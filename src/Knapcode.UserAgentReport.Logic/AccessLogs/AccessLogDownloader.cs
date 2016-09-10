using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Renci.SshNet;

namespace Knapcode.UserAgentReport.AccessLogs
{
    public class AccessLogDownloader
    {
        private readonly IAccessLogParser _parser;

        public AccessLogDownloader(IAccessLogParser parser)
        {
            _parser = parser;
        }

        public IEnumerable<AccessLogEntry> GetLogEntryPages(ConnectionInfo connectionInfo, string remotePattern)
        {
            // get the list of files via SSH
            var remoteAccessLogList = GetRemoteAccessLogs(connectionInfo, remotePattern);

            using (var scpClient = new ScpClient(connectionInfo))
            {
                scpClient.Connect();
                
                foreach (var remotePath in remoteAccessLogList)
                {
                    var entries = new Stack<AccessLogEntry>();

                    // write the file to the temporary directory
                    string localPath = Path.GetTempFileName();
                    using (var localStream = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        // download the log file
                        scpClient.Download(remotePath, localStream);

                        // reset the file cursor to the beginning of the file
                        Stream readStream = localStream;
                        readStream.Position = 0;

                        // decompress the file, if necessary
                        if (remotePath.EndsWith(".gz"))
                        {
                            readStream = new GZipStream(localStream, CompressionMode.Decompress);
                        }

                        // read the file line by line
                        using (var reader = new StreamReader(readStream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                entries.Push(_parser.ParseLine(line));
                            }
                        }
                    }

                    // remove the local file
                    File.Delete(localPath);

                    // yield the results
                    foreach (var entry in entries)
                    {
                        yield return entry;
                    }
                }
            }
        }

        private IEnumerable<string> GetRemoteAccessLogs(ConnectionInfo connectionInfo, string remotePattern)
        {
            // get the list of access logs via SSH
            using (var sshClient = new SshClient(connectionInfo))
            {
                sshClient.Connect();

                string output = sshClient.CreateCommand(string.Format("ls -mt {0}", remotePattern)).Execute();
                var remoteAccessLogList = output
                    .Split(',')
                    .Select(p => p.Trim())
                    .ToArray();

                return remoteAccessLogList;
            }
        }
    }
}