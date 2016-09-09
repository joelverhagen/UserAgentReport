using System.Threading;
using System.Threading.Tasks;
using Knapcode.UserAgentReport.Reporting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Knapcode.UserAgentReport.WebApi.Controllers
{
    public class ManagementController : Controller
    {
        private readonly UserAgentDatabaseUpdater _updater;
        private readonly IOptions<WebsiteSettings> _options;

        public ManagementController(IOptions<WebsiteSettings> options, UserAgentDatabaseUpdater updater)
        {
            _options = options;
            _updater = updater;
        }

        [HttpPost("api/v1/management/update-user-agent-database")]
        public async Task<IActionResult> UpdateUserAgentDatabaseAsync(
            [FromHeader] string accessToken = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrEmpty(_options.Value.AccessToken) &&
                accessToken != _options.Value.AccessToken)
            {
                return Unauthorized();
            }

            var result = await _updater.UpdateAsync(cancellationToken);

            return new ObjectResult(result);
        }

        [HttpGet("api/v1/management/user-agent-database-status")]
        public UserAgentDatabaseStatus GetUserAgentDatabaseStatus()
        {
            return _updater.GetStatus();
        }
    }
}