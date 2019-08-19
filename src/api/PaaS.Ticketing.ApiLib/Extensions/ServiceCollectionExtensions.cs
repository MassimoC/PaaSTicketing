using IdentityServer4.AccessTokenValidation;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PaaS.Ticketing.ApiLib.Context;
using PaaS.Ticketing.ApiLib.Factories;
using PaaS.Ticketing.ApiLib.Filters;
using PaaS.Ticketing.ApiLib.Models;
using PaaS.Ticketing.ApiLib.OpenApi;
using PaaS.Ticketing.ApiLib.Repositories;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static PaaS.Ticketing.ApiLib.ContentTypeNames;

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
            services?.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Type = context.HttpContext.Request.Path,
                        Title = "Validation error",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = Constants.Messages.ProblemDetailsDetail,
                        Instance = $"urn:{Constants.API.CompanyName}:{Constants.API.ClientError}:{Activity.Current.Id}"
                    };
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { ContentTypeNames.Application.JsonProblem, ContentTypeNames.Application.XmlProblem }
                    };
                };
            });
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

                var inputFormatter = cfg.InputFormatters.OfType<JsonInputFormatter>().FirstOrDefault();
                if (inputFormatter != null)
                {
                    if (inputFormatter.SupportedMediaTypes.Contains(Text.Json))
                    {
                        inputFormatter.SupportedMediaTypes.Remove(Text.Json);
                    }
                    if (inputFormatter.SupportedMediaTypes.Contains(Text.Plain))
                    {
                        inputFormatter.SupportedMediaTypes.Remove(Text.Plain);
                    }
                }

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
                var stringFormatter = cfg.OutputFormatters.OfType<StringOutputFormatter>().FirstOrDefault();
                if (stringFormatter != null) cfg.OutputFormatters.RemoveType<StringOutputFormatter>();

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
        public static void ConfigureOpenApiGeneration(this IServiceCollection services, bool enableExamples = false)
        {
            var xmlDocumentationPath = GetXmlDocumentationPath(services);

            services?.AddSwaggerGen(swaggerGenOptions =>
            {
                swaggerGenOptions.EnableAnnotations();
                swaggerGenOptions.DescribeAllEnumsAsStrings();
                swaggerGenOptions.SwaggerDoc(name: Constants.OpenApi.ApiVersion, info: new Info
                {
                    Version = Constants.OpenApi.ApiVersion,
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
                if (enableExamples) swaggerGenOptions.ExampleFilters();
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

                swaggerGenOptions.CustomSchemaIds(PublicSchemaIds);
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
            services.AddSingleton<ITelemetryInitializer, CloudRoleInitializer>();
            services.AddApplicationInsightsTelemetry(aiOptions);
            services.ConfigureTelemetryModule<Microsoft.ApplicationInsights.AspNetCore.RequestTrackingTelemetryModule>
                ((req, o) => req.CollectionOptions.TrackExceptions = false);

            services.AddSingleton<ITelemetryClientFactory, TelemetryClientFactory>();
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

        private static string PublicSchemaIds(Type currentClass)
        {
            string returnedValue = currentClass.Name;
            if (returnedValue.EndsWith("Dto"))
                returnedValue = returnedValue.Replace("Dto", string.Empty);
            return returnedValue;
        }
    }
}