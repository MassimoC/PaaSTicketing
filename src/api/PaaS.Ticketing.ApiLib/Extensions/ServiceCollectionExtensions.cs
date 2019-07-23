using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PaaS.Ticketing.ApiLib.Context;
using PaaS.Ticketing.ApiLib.Repositories;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Linq;
using static PaaS.Ticketing.ApiLib.ContentTypeNames;
using IdentityServer4.AccessTokenValidation;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using PaaS.Ticketing.ApiLib.Models;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using PaaS.Ticketing.ApiLib.Filters;
using System;
using PaaS.Ticketing.ApiLib.OpenApi;
using Microsoft.ApplicationInsights.Extensibility;

namespace PaaS.Ticketing.ApiLib.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Configure database
        /// </summary>
        /// <param name="services">Collections of services in application</param>
        /// <param name="configuration">Configuration properties</param>
        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                return;
            }
            var connectionString = configuration.GetConnectionString(name: "TicketingDB");
            //default scope lifetime
            //#if DEBUG
            //            services.AddDbContext<ConcertsContext>(opt => opt.UseInMemoryDatabase(databaseName: "TicketingDB"));
            //#else
            //            services.AddDbContext<ConcertsContext>(o => o.UseSqlServer(connectionString)); 
            //#endif
            services.AddDbContext<TicketingContext>(o => o.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                    sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                    })              
                );
            services.AddScoped<IConcertsRepository, ConcertsRepository>(); //scoped
            services.AddScoped<IUsersRepository, UsersRepository>(); //scoped
            services.AddScoped<IOrdersRepository, OrdersRepository>(); //scoped
        }

        public static void ConfigureIdp(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                return;
            }
            // Add the IdentityProvider object so it can be injected
            services.Configure<IdentityProvider>(configuration.GetSection("IdentityProvider"));

            var idpAuthority = configuration.GetSection("IdentityProvider:IdpAuthorityURL").Value;
            var idpApiName = configuration.GetSection("IdentityProvider:IdpApiName").Value;

            GuardNet.Guard.NotNullOrEmpty(idpAuthority, nameof(idpAuthority));
            GuardNet.Guard.NotNullOrEmpty(idpApiName, nameof(idpApiName));
            services.AddAuthentication(
                IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = idpAuthority;
                    options.ApiName = idpApiName;
                });

        }

        /// <summary>
        ///     Configure how to handle invalid state with problem+json
        /// </summary>
        /// <param name="services">Collections of services in application</param>
        public static void ConfigureInvalidStateHandling(this IServiceCollection services)
        {

        }

        /// <summary>
        ///     Configure the MVC stack
        /// </summary>
        /// <param name="services">Collections of services in application</param>
        public static void ConfigureMvc(this IServiceCollection services)
        {
            services?.AddMvc(cfg =>
            {
                cfg.RespectBrowserAcceptHeader = true;
                cfg.ReturnHttpNotAcceptable = true; // Return 406 for not acceptable media types

                var outputFormatter = cfg.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                if (outputFormatter != null)
                {
                    if (outputFormatter.SupportedMediaTypes.Contains(Text.Json))
                    {
                        outputFormatter.SupportedMediaTypes.Remove(Text.Json);
                    }
                    if (outputFormatter.SupportedMediaTypes.Contains(Text.Plain))
                    {
                        outputFormatter.SupportedMediaTypes.Remove(Text.Plain);
                    }
                }
                cfg.Filters.Add(typeof(LogBodyActionFilter));
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(opt =>
                {
                    //explicit datetime configuration
                    opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    opt.SerializerSettings.DateFormatString = "o";
                    opt.SerializerSettings.Converters.Add(new StringEnumConverter
                    {
                        CamelCaseText = false
                    });
                });
        }

        /// <summary>
        ///     Configure OpenAPI generation
        /// </summary>
        /// <param name="services">Collections of services in application</param>
        public static void ConfigureOpenApiGeneration(this IServiceCollection services)
        {
            var xmlDocumentationPath = GetXmlDocumentationPath(services);

            services?.AddSwaggerGen(swaggerGenOptions =>
            {
                swaggerGenOptions.EnableAnnotations();
                swaggerGenOptions.DescribeAllEnumsAsStrings();
                swaggerGenOptions.SwaggerDoc(name: "v1", info: new Info
                {
                    Version = "v1",
                    Title = Constants.OpenApi.Title,
                    Description = Constants.OpenApi.Description,
                    TermsOfService = Constants.OpenApi.TermsOfService,
                    Contact = new Contact
                    {
                        Name = Constants.OpenApi.ContactName,
                        Email = Constants.OpenApi.ContactEmail,
                        Url = Constants.OpenApi.ContactUrl
                    }
                });
                swaggerGenOptions.AddSecurityDefinition("Bearer",
                    new ApiKeyScheme
                    {
                        In = "header",
                        Description = "Please enter into field the word 'Bearer' following by space and JWT token",
                        Name = "Authorization",
                        Type = "apiKey"
                    });
                swaggerGenOptions.OperationFilter<SecurityRequirementsOperationFilter>();
                swaggerGenOptions.DocumentFilter<OpenApiDocumentFilter>();

                //swaggerGenOptions.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                //{
                //    //{ "oauth2", new[] { "orders:write", "users:write" } }
                //    { "Bearer", Enumerable.Empty<string>() }
                //});

                if (string.IsNullOrEmpty(xmlDocumentationPath) == false)
                {
                    swaggerGenOptions.IncludeXmlComments(xmlDocumentationPath);
                }

                swaggerGenOptions.CustomSchemaIds(publicSchemaIds);
            });

        }

        /// <summary>
        ///     Configure routing
        /// </summary>
        public static void ConfigureRouting(this IServiceCollection services)
        {
            services.AddRouting(configureOptions => configureOptions.LowercaseUrls = true);
        }

        /// <summary>
        ///     Configure application Insights
        /// </summary>
        public static void ConfigureAI(this IServiceCollection services)
        {
            ApplicationInsightsServiceOptions aiOptions = new ApplicationInsightsServiceOptions
            {
                EnableDebugLogger = false
            };
            // TODO : commented out 
            //services.AddSingleton<ITelemetryInitializer, CloudRoleInitializer>();
            services.AddApplicationInsightsTelemetry(aiOptions);
            services.ConfigureTelemetryModule<Microsoft.ApplicationInsights.AspNetCore.RequestTrackingTelemetryModule>
                ((req, o) => req.CollectionOptions.TrackExceptions = false);
        }

        private static string GetXmlDocumentationPath(IServiceCollection services)
        {
            var hostingEnvironment = services.FirstOrDefault(service => service.ServiceType == typeof(IHostingEnvironment));
            if (hostingEnvironment == null)
            {
                return string.Empty;
            }

            var contentRootPath = ((IHostingEnvironment)hostingEnvironment.ImplementationInstance).ContentRootPath;
            var xmlDocumentationPath = $"{contentRootPath}/OpenApi-Docs.xml";

            return File.Exists(xmlDocumentationPath) ? xmlDocumentationPath : string.Empty;
        }

        private static string publicSchemaIds(Type currentClass)
        {
            string returnedValue = currentClass.Name;
            if (returnedValue.EndsWith("Dto"))
                returnedValue = returnedValue.Replace("Dto", string.Empty);
            return returnedValue;
        }
    }
}