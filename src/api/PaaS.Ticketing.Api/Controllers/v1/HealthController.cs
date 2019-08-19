using Microsoft.AspNetCore.Mvc;
using PaaS.Ticketing.Api.Examples;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace PaaS.Ticketing.Api.Controllers.v1
{
    /// <summary>
    /// Health endpoint for availability tests
    /// </summary>
    [Route("core/v1/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        ///     Get Health
        /// </summary>
        /// <remarks>Provides an indication about the health of the runtime</remarks>
        [HttpGet(Name = "Health_Get")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [SwaggerResponse((int)HttpStatusCode.OK, "Runtime is up and running in a healthy state")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Runtime is not healthy", typeof(ProblemDetails))]
        [SwaggerResponseExample((int)HttpStatusCode.InternalServerError, typeof(DocProblemDetail500))]

        public IActionResult Get()
        {
            return Ok();
        }
    }
}