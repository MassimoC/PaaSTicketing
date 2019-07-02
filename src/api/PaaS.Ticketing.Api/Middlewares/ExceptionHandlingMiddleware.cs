using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PaaS.Ticketing.Api.Extensions;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PaaS.Ticketing.Api.Middlewares
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

            //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var errorDetail = context.Request.IsLocalRequest()
                ? ex.Demystify().ToString()
                : "The instance value should be used to identify the problem when calling customer support";

            var problemDetails = new ProblemDetailsError
            {
                Title = "An unexpected error occurred!",
                Status = StatusCodes.Status500InternalServerError,
                Detail = errorDetail,
                Instance = $"urn:codit.eu:server-error:{Guid.NewGuid()}"
            };

            //TODO headers
            context.Response.WriteJson(problemDetails, contentType: ContentTypeNames.Application.JsonProblem);
        }
    }
}
