using Lykke.Common;
using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.Net;

namespace Lykke.Sdk.Controllers
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    /// <summary>
    /// Common controller for app health monitoring.
    /// </summary>
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        /// <summary>
        /// Checks service is alive
        /// </summary>
        [HttpGet]
        [SwaggerOperation("IsAlive")]
        [ProducesResponseType(typeof(IsAliveResponse), (int)HttpStatusCode.OK)]
        public IActionResult Get()
        {
            // NOTE: Feel free to extend IsAliveResponse, to display job-specific indicators
            return Ok(new IsAliveResponse
            {
                Name = AppEnvironment.Name,
                Version = AppEnvironment.Version,
                Env = AppEnvironment.EnvInfo,
                IsDebug = Debugger.IsAttached,
            });
        }
    }
}
