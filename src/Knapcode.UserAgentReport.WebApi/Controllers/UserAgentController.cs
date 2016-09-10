using System.Collections.Generic;
using Knapcode.UserAgentReport.Reporting;
using Microsoft.AspNetCore.Mvc;

namespace Knapcode.UserAgentReport.WebApi.Controllers
{
    public class UserAgentController : Controller
    {
        private readonly UserAgentDatabase _database;

        public UserAgentController(UserAgentDatabase database)
        {
            _database = database;
        }

        [HttpGet("api/v1/top-user-agents")]
        public IEnumerable<TopUserAgent> GetTopUserAgentsAsync(int limit = 10, bool bots = false, bool browsers = true)
        {
            return _database.GetTopUserAgents(limit, bots, browsers);
        }
    }
}