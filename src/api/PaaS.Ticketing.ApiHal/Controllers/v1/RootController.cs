using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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