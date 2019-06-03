using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;

namespace PaaS.Ticketing.Api.Filters
{
    public class LogBodyActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.HttpContext.Request.Method == HttpMethod.Post.ToString() 
                || actionContext.HttpContext.Request.Method == HttpMethod.Put.ToString()
                || actionContext.HttpContext.Request.Method == HttpMethod.Patch.ToString())
            {
                var tclient = new TelemetryClient(TelemetryConfiguration.Active);

                if (actionContext.HttpContext.Request.Body == null)
                {
                    tclient.TrackTrace("-empty body-");
                    return;
                }
                using (var stream = new StreamReader(actionContext.HttpContext.Request.Body))
                {
                    stream.BaseStream.Position = 0;   
                    tclient.TrackTrace(JObject.Parse(stream.ReadToEnd()).ToString(Newtonsoft.Json.Formatting.None));
                    
                }
            }
        }

        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            // TODO log responses
            //var jsonResult = (JsonResult) actionExecutedContext.Result;
            //var jsonResponse = actionExecutedContext.Exception != null
            //    ? actionExecutedContext.Exception.Message
            //    : jsonResult.ToString();

            //var tclient = new TelemetryClient(TelemetryConfiguration.Active);
            //tclient.TrackTrace(jsonResponse);
        }
    }
}
