using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Knapcode.UserAgentReport.Reporting
{
    public static class JsonSerialization
    {
        public static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters =
            {
                new StringEnumConverter(),
                new IsoDateTimeConverter()
            }
        };

        public static JsonSerializer Serializer => JsonSerializer.Create(SerializerSettings);
    }
}