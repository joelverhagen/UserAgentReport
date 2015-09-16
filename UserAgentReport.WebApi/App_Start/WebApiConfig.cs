using System.IO;
using System.Web;
using System.Web.Http;
using Knapcode.UserAgentReport.Reporting;

namespace Knapcode.UserAgentReport.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.Clear();
            config.Formatters.Add(JsonSerialization.MediaTypeFormatter);

            // Web API routes
            config.MapHttpAttributeRoutes();

            // create App_Data
            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/App_Data"));
        }
    }
}