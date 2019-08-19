using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PaaS.Ticketing.ApiLib;
using PaaS.Ticketing.ApiLib.Context;
using PaaS.Ticketing.ApiLib.Extensions;
using PaaS.Ticketing.ApiLib.Factories;
using PaaS.Ticketing.ApiLib.Filters;
using PaaS.Ticketing.ApiLib.Middlewares;
using PaaS.Ticketing.Security;
using System.Linq;
using static PaaS.Ticketing.ApiLib.ContentTypeNames;

namespace PaaS.Ticketing.ApiHal
{
    public class Startup
    {
        private readonly ILogger _logger;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add functionality to inject IOptions<T>
            services.AddOptions();
            // Application Insights config
            services.ConfigureAI();
            _logger.LogInformation("Startup - Configuring services (HAL)...");

            // Configure DB
            services.ConfigureDatabase(Configuration);

            // Configure Identity Provider
            services.ConfigureIdp(Configuration);

            // Configure API
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
                //.AddApplicationPart(Assembly.Load(new AssemblyName("PaaS.Ticketing.ApiLib")))
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


            services.ConfigureOpenApiGeneration();
            services.ConfigureRouting();
            services.ConfigureInvalidStateHandling();
            services.AddSingleton<ITelemetryClientFactory, TelemetryClientFactory>();
            services.AddSingleton<IVaultService>(new VaultService(Configuration["Security:VaultName"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, TicketingContext concertContext)
        {
            _logger.LogInformation("Startup - Configuring app...");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Seed DB
            //concertContext.DataSeed();

            // Configure API
            app.UseMiddleware<ScopedLoggingMiddleware>();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            // prefer the middleware approach
            //app.UseExceptionHandlerWithProblemJson();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            //app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseMvc();
            app.UseOpenApi();

            // Configure Automapper
            AutoMapperConfig.Initialize();
        }
    }
}
