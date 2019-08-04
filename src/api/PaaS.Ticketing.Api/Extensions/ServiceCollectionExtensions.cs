using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;

namespace PaaS.Ticketing.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureOpenApiExamples(this IServiceCollection services)
        {
            services.AddSwaggerExamples();
            // services.AddSwaggerExamplesFromAssemblyOf<DocOrderCreateDto>();
        }
    }
}
