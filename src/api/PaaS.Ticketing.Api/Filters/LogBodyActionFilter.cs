using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;

namespace PaaS.Ticketing.Api.Filters
{
    public class LogBodyActionFilter : ActionFilterAttribute
    {
        private TelemetryClient _client;

        public LogBodyActionFilter()
        {
            _client = new TelemetryClient(TelemetryConfiguration.Active);
        }
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.HttpContext.Request.Method == HttpMethod.Post.ToString()
                || actionContext.HttpContext.Request.Method == HttpMethod.Put.ToString()
                || actionContext.HttpContext.Request.Method == HttpMethod.Patch.ToString())
            {
                if (actionContext.HttpContext.Request.Body == null)
                {
                    _client.TrackTrace("-empty body-");
                    return;
                }
                using (var stream = new StreamReader(actionContext.HttpContext.Request.Body))
                {
                    stream.BaseStream.Position = 0;
                    _client.TrackTrace("REQUEST BODY:" + JObject.Parse(stream.ReadToEnd()).ToString(Newtonsoft.Json.Formatting.None));
                }
            }
        }

        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            // this fires after the engine has written its response to the client. Response is not readible.
            // use a middleware
        }
    }
}
