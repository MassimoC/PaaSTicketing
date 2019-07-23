using Microsoft.AspNetCore.Mvc.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib.Filters
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controllerAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true);
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);

            if (isAuthorized)
            {
                operation.Security = new List<IDictionary<string, IEnumerable<string>>>{
                    new Dictionary<string, IEnumerable<string>>
                    {
                        { "Bearer", Enumerable.Empty<string>() }
                    }
                };
            }
        }
    }
}
