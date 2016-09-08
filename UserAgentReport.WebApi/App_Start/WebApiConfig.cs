using System.IO;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Knapcode.UserAgentReport.WebApi
{
    public static class WebApiConfig
    {
        public static JsonMediaTypeFormatter JsonMediaTypeFormatter => new JsonMediaTypeFormatter
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

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.Clear();
            config.Formatters.Add(JsonMediaTypeFormatter);

            // Web API routes
            config.MapHttpAttributeRoutes();

            // create App_Data
            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/App_Data"));
        }
    }
}