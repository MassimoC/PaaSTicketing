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
        /// <summary>
        /// Host, base path and schemes configuration
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            //swaggerDoc.Host = "readfromconfig.azurewebsites.net";
            swaggerDoc.BasePath = "/";
            swaggerDoc.Schemes = new List<string> { "https" };
        }
    }
}
