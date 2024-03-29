﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Diagnostics;
using System.Linq;

namespace PaaS.Ticketing.ApiLib.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Configure to use OpenAPI with UI
        /// </summary>
        /// <param name="applicationbuilder">Application builder to use</param>
        public static void UseOpenApi(this IApplicationBuilder applicationbuilder)
        {
            applicationbuilder.UseSwagger(UseLowercaseUrls);
            applicationbuilder.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint(url: $"/swagger/{Constants.OpenApi.ApiVersion}/swagger.json", name: Constants.OpenApi.Title);
                opt.DisplayOperationId();
                opt.EnableDeepLinking();
                opt.DocumentTitle = Constants.OpenApi.Title;
                opt.DocExpansion(DocExpansion.List);
                opt.DisplayRequestDuration();
                opt.EnableFilter();
            });
        }

        // Makes sure that the urls are lower case. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/74
        private static void UseLowercaseUrls(SwaggerOptions swaggerOptions)
        {
            swaggerOptions.PreSerializeFilters.Add((document, request) => { document.Paths = document.Paths.ToDictionary(p => p.Key.ToLowerInvariant(), p => p.Value); });
        }

        /// <summary>
        ///     Configure to use global exception handler with application/problem+json
        /// </summary>
        /// <param name="applicationBuilder">Application builder to use</param>
        public static void UseExceptionHandlerWithProblemJson(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder?.UseExceptionHandler(errorApplication =>
            {
                errorApplication.Run(async context =>
                {
                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = errorFeature.Error;

                    var errorDetail = context.Request.IsLocalRequest()
                        ? exception.Demystify().ToString()
                        : "The instance value should be used to identify the problem when calling customer support";

                    var problemDetails = new ProblemDetailsError
                    {
                        Title = "An unexpected error occurred!",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = errorDetail,
                        Instance = $"urn:{Constants.API.CompanyName}:{Constants.API.ServerError}:{Activity.Current.Id}"
                    };

                    // TODO: headers are not propagated
                    // TODO: Plug in telemetry
                    context.Response.WriteJson(problemDetails, contentType: ContentTypeNames.Application.JsonProblem);
                });
            });
        }

    }
}