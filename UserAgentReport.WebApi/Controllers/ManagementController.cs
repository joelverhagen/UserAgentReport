using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Knapcode.UserAgentReport.Reporting;
using Knapcode.UserAgentReport.WebApi.BusinessLogic;

namespace Knapcode.UserAgentReport.WebApi.Controllers
{
    public class ManagementController : ApiController
    {
        private readonly UserAgentDatabaseUpdater _updater;

        public ManagementController()
        {
            _updater = Singletons.UserAgentDatabaseUpdater;
        }

        [HttpPost, Route("api/v1/management/update-user-agent-database")]
        public async Task<UserAgentDatabaseStatus> UpdateUserAgentDatabaseAsync(CancellationToken cancellationToken)
        {
            return await _updater.UpdateAsync(cancellationToken);
        }

        [HttpGet, Route("api/v1/management/user-agent-database-status")]
        public UserAgentDatabaseStatus GetUserAgentDatabaseStatus()
        {
            return _updater.GetStatus();
        }
    }
}