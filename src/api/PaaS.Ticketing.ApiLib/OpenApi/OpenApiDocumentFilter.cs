using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace PaaS.Ticketing.ApiLib.OpenApi
{
    /// <summary>
    /// OpenApi document filter
    /// </summary>
    public class OpenApiDocumentFilter : IDocumentFilter
    {

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Host, base path and schemes configuration
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var hostName = (_configuration["ASPNETCORE_ENVIRONMENT"] != "Production")
                 ? "localhost"
                 : $"{_configuration["WEBSITE_SITE_NAME"]}.azurewebsites.net";
            
            swaggerDoc.Host = hostName;
            swaggerDoc.BasePath = "/";
            swaggerDoc.Schemes = new List<string> { "https" };
        }

        public OpenApiDocumentFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
