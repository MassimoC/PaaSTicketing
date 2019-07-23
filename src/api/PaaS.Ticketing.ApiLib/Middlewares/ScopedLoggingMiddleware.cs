using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PaaS.Ticketing.ApiLib.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib.Middlewares
{
    public class ScopedLoggingMiddleware
    {
        // https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/HttpCorrelationProtocol.md
        const string CORRELATION_ID_HEADER_NAME = "X-Request-Id";
        private readonly RequestDelegate next;
        private readonly ILogger<ScopedLoggingMiddleware> _logger;

        public ScopedLoggingMiddleware(RequestDelegate next, ILogger<ScopedLoggingMiddleware> logger)
        {
            this.next = next ?? throw new System.ArgumentNullException(nameof(next));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            string correlationId = GetOrAddCorrelationHeader(context);
            try
            {
                var loggerState = new LoggerState
                {
                    [CORRELATION_ID_HEADER_NAME] = correlationId
                    //Add any number of properties to be logged under a single scope
                };

                using (_logger.BeginScope(loggerState))
                {
                    await next(context);
                }
            }
            //do not loose the scope in case of an unexpected error
            catch (Exception ex) when (LogOnUnexpectedError(ex))
            {
                return;
            }
        }

        private string GetOrAddCorrelationHeader(HttpContext context)
        {
            // TODO : make this RFC compliant
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(context.Request.Headers[CORRELATION_ID_HEADER_NAME]))
            { 
                context.Request.Headers.Add(CORRELATION_ID_HEADER_NAME, Guid.NewGuid().ToString());
                context.Response.Headers.Add("Request-Id", Activity.Current.Id);
            }

            return context.Request.Headers[CORRELATION_ID_HEADER_NAME];
        }

        private bool LogOnUnexpectedError(Exception ex)
        {
            _logger.LogError(ex, "An unexpected exception occured!");
            return true;
        }
    }
}
