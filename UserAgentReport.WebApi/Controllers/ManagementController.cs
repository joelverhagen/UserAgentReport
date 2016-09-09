using System.Threading;
using System.Threading.Tasks;
using Knapcode.UserAgentReport.Reporting;
using Microsoft.AspNetCore.Mvc;

namespace Knapcode.UserAgentReport.WebApi.Controllers
{
    public class ManagementController : Controller
    {
        private readonly UserAgentDatabaseUpdater _updater;

        public ManagementController(UserAgentDatabaseUpdater updater)
        {
            _updater = updater;
        }

        [HttpPost("api/v1/management/update-user-agent-database")]
        public async Task<UserAgentDatabaseStatus> UpdateUserAgentDatabaseAsync(CancellationToken cancellationToken)
        {
            return await _updater.UpdateAsync(cancellationToken);
        }

        [HttpGet("api/v1/management/user-agent-database-status")]
        public UserAgentDatabaseStatus GetUserAgentDatabaseStatus()
        {
            return _updater.GetStatus();
        }
    }
}