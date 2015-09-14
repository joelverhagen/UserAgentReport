using System.Web;
using System.Web.Http;

namespace Knapcode.UserAgentReport.WebApi
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
