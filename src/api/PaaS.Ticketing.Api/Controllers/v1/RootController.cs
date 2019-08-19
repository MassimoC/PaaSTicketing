using Microsoft.AspNetCore.Mvc;

namespace PaaS.Ticketing.Api.Controllers.v1
{
    /// <summary>
    /// API root
    /// </summary>
    [Route("/")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Get()
        {
            return Ok("Root controller");
        }
    }
}