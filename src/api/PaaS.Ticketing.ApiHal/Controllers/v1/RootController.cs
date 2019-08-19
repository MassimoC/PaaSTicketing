using Microsoft.AspNetCore.Mvc;

namespace PaaS.Ticketing.ApiHal.Controllers.v1
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
            return Ok("HAL - Hey there!");
        }
    }
}