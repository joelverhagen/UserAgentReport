﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Knapcode.UserAgentReport.WebApi.BusinessLogic;

namespace Knapcode.UserAgentReport.WebApi.Controllers
{
    public class UserAgentController : ApiController
    {
        private readonly UserAgentDatabase _database;
        private readonly UserAgentDatabaseUpdater _updater;

        public UserAgentController()
        {
            _database = Singletons.UserAgentDatabase;
            _updater = Singletons.UserAgentDatabaseUpdater;
        }

        [HttpGet, Route("api/v1/top-user-agents")]
        public async Task<IEnumerable<UserAgentAndCount>> GetTopUserAgentsAsync(
            CancellationToken cancellationToken,
            int limit = 50,
            bool bots = false,
            bool browsers = true)
        {
            if (_updater.GetStatus().Type != UserAgentDatabaseStatusType.Updated)
            {
                await _updater.UpdateAsync(cancellationToken);
            }

            return _database.GetTopUserAgents(limit, bots, browsers);
        }
    }
}