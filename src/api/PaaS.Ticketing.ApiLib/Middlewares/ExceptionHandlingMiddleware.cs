using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaaS.Ticketing.ApiLib.Extensions;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Func<string> _getLoggingCategory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        public ExceptionHandlingMiddleware(RequestDelegate next)
            : this(next, string.Empty)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the asp.net core request pipeline.</param>
        /// <param name="categoryName">The category-name for messages produced by the logger.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, string categoryName)
            : this(next, () => categoryName)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the asp.net core request pipeline.</param>
        /// <param name="getLoggingCategory">The function that returns the category-name that must be used by the logger
        /// when writing log messages.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, Func<string> getLoggingCategory)
        {
            _next = next;
            _getLoggingCategory = getLoggingCategory;
        }

        public async Task Invoke(HttpContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                HandleException(context, ex, loggerFactory);
            }
        }

        private void HandleException(HttpContext context, Exception ex, ILoggerFactory loggerFactory)
        {
            string categoryName = _getLoggingCategory() ?? string.Empty;

            var logger = loggerFactory.CreateLogger(categoryName);
            logger.LogCritical(ex, ex.Message);

            var errorDetail = context.Request.IsLocalRequest()
                ? ex.Demystify().ToString()
                : "The instance value should be used to identify the problem when calling customer support";

            var problemDetails = new ProblemDetailsError
            {
                Title = "An unexpected error occurred!",
                Status = StatusCodes.Status500InternalServerError,
                Detail = errorDetail,
                Instance = $"urn:{Constants.API.CompanyName}:{Constants.API.ServerError}:{Activity.Current.Id}"
            };

            problemDetails.Extensions.Add("traceId", context.TraceIdentifier);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.Headers.Add("Content-Type", String.Format("{0}; {1}", ContentTypeNames.Application.JsonProblem, "charset=utf-8"));
            context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(problemDetails)));
        }
    }
}
