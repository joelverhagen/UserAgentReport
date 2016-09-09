using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Knapcode.UserAgentReport.Reporting
{
    public class UserAgentDatabaseUpdater
    {
        private const int BufferSize = 4096;
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);
        private readonly UserAgentDatabaseUpdaterSettings _settings;

        public UserAgentDatabaseUpdater(UserAgentDatabaseUpdaterSettings settings)
        {
            _settings = settings;
        }

        public async Task<UserAgentDatabaseStatus> UpdateAsync(CancellationToken cancellationToken)
        {
            // get the database
            var client = new HttpClient();
            using (var response = await client.GetAsync(_settings.DatabaseUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var networkStream = await response.Content.ReadAsStreamAsync();

                // aquire a lock
                await SemaphoreSlim.WaitAsync(cancellationToken);
                using (new SemaphoreSlimHandle(SemaphoreSlim))
                {
                    // set the status to updating
                    await WriteStatusAsync(UserAgentDatabaseStatusType.Updating);

                    var temporaryDatabasePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                    // download the database
                    using (networkStream)
                    using (var fileStream = new FileStream(
                        temporaryDatabasePath,
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.Read,
                        BufferSize,
                        FileOptions.DeleteOnClose | FileOptions.Asynchronous))
                    {
                        await networkStream.CopyToAsync(fileStream, BufferSize, cancellationToken);

                        File.Copy(temporaryDatabasePath, _settings.DatabasePath, overwrite: true);
                    }

                    // set the status to updated
                    return await WriteStatusAsync(UserAgentDatabaseStatusType.Updated);
                }
            }
        }

        private async Task<UserAgentDatabaseStatus> WriteStatusAsync(UserAgentDatabaseStatusType type)
        {
            using (var fileStream = new FileStream(_settings.StatusPath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(fileStream, new UTF8Encoding(false)))
            {
                var status = new UserAgentDatabaseStatus {LastUpdated = DateTimeOffset.UtcNow, Type = type};
                var statusJson = JsonConvert.SerializeObject(status, JsonSerialization.SerializerSettings);
                await writer.WriteAsync(statusJson);
                return status;
            }
        }

        public UserAgentDatabaseStatus GetStatus()
        {
            if (!File.Exists(_settings.StatusPath) || !File.Exists(_settings.DatabasePath))
            {
                return new UserAgentDatabaseStatus {LastUpdated = DateTimeOffset.UtcNow, Type = UserAgentDatabaseStatusType.Unavailable};
            }

            // detect stale state
            var status = GetStatusFromFile();
            if (status.Type == UserAgentDatabaseStatusType.Updated && (!status.LastUpdated.HasValue || DateTimeOffset.UtcNow - status.LastUpdated.Value > _settings.RefreshPeriod))
            {
                status.Type = UserAgentDatabaseStatusType.Stale;
            }

            return status;
        }

        private UserAgentDatabaseStatus GetStatusFromFile()
        {
            using (var stream = new FileStream(_settings.StatusPath, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return JsonSerialization.Serializer.Deserialize<UserAgentDatabaseStatus>(jsonTextReader);
            }
        }
    }
}