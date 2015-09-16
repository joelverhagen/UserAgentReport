using System.Collections.Generic;
using System.Web.Http;
using Knapcode.UserAgentReport.Reporting;
using Knapcode.UserAgentReport.WebApi.BusinessLogic;

namespace Knapcode.UserAgentReport.WebApi.Controllers
{
    public class UserAgentController : ApiController
    {
        private readonly UserAgentDatabase _database;

        public UserAgentController()
        {
            _database = Singletons.UserAgentDatabase;
        }

        [HttpGet, Route("api/v1/top-user-agents")]
        public IEnumerable<TopUserAgent> GetTopUserAgentsAsync(int limit = 10, bool bots = false, bool browsers = true)
        {
            return _database.GetTopUserAgents(limit, bots, browsers);
        }
    }
}