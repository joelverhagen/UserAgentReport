using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Knapcode.UserAgentReport.WebApi.BusinessLogic
{
    public static class JsonSerialization
    {
        public static JsonSerializerSettings SerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters =
                    {
                        new StringEnumConverter(),
                        new IsoDateTimeConverter()
                    }
                };
            }
        }

        public static JsonSerializer Serializer
        {
            get
            {
                return JsonSerializer.Create(SerializerSettings);
            }
        }

        public static JsonMediaTypeFormatter MediaTypeFormatter
        {
            get
            {
                return new JsonMediaTypeFormatter
                {
                    SerializerSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Converters =
                        {
                            new StringEnumConverter(),
                            new IsoDateTimeConverter()
                        }
                    },
                    UseDataContractJsonSerializer = false
                };
            }
        }
    }
}