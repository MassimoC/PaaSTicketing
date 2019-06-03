using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PaaS.Ticketing.Api.Filters
{
    public class LoggingFilterAttribute : ActionFilterAttribute
    {
        private const string RequestDate = "StartRequestDate";
        private TelemetryClient _client;

        public LoggingFilterAttribute()
        {
            _client = new TelemetryClient(TelemetryConfiguration.Active);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
        }


        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var trackRequest = new RequestTelemetry();

            var response = context.HttpContext.Response;
            var exception = context?.Exception;
            var request = context?.HttpContext.Request;

            if (response != null)
            {
                DateTime startDate = (DateTime)context.HttpContext.Items[RequestDate];

                TimeSpan time = DateTime.UtcNow - startDate;

                trackRequest.Name = request.Path;
                trackRequest.Timestamp = startDate;
                trackRequest.Duration = time;
                trackRequest.ResponseCode = response.StatusCode.ToString();
                trackRequest.Success = exception == null;
                trackRequest.Properties.Add("Source", GetType().Assembly.FullName);
                trackRequest.Properties.Add("Timestamp", startDate.ToString());
                trackRequest.Properties.Add("Url", context.HttpContext.Request.Path.ToString());
                trackRequest.Properties.Add("Path", context.HttpContext.Request.Path.Value);
                trackRequest.Properties.Add("HttpMethod", context.HttpContext.Request?.Method);

                foreach (var logProperty in context.HttpContext.Request.Headers)
                {
                    if (logProperty.Key != null && !string.IsNullOrEmpty(logProperty.Value))
                    {
                        trackRequest.Properties.Add(logProperty.Key, logProperty.Value);
                    }
                }

                _client.TrackRequest(trackRequest);
            }
            else if (exception != null)
            {
                _client.TrackTrace("Failed to process request.", SeverityLevel.Error);
                _client.TrackException(exception);
            }
            else
            {
                base.OnActionExecuted(context);
            }
        }


        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.Items.Add(RequestDate, DateTime.UtcNow);

            return base.OnActionExecutionAsync(context, next);
        }
    }
}
