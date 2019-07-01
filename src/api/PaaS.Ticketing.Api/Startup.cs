using IdentityServer4.AccessTokenValidation;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaaS.Ticketing.Api.Context;
using PaaS.Ticketing.Api.Extensions;
using PaaS.Ticketing.Api.Factories;
using PaaS.Ticketing.Api.Middlewares;
using PaaS.Ticketing.Security;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api
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

        // Add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add functionality to inject IOptions<T>
            services.AddOptions();
            // Application Insights config
            services.ConfigureAI();
            _logger.LogInformation("Startup - Configuring services...");

            // for migrations
            //var cn = Configuration.GetConnectionString(name: "TicketingDB");
            //services.AddDbContext<TicketingContext>(o => o.UseSqlServer(cn));

            // Configure DB
            services.ConfigureDatabase(Configuration);

            // Configure Identity Provider
            services.ConfigureIdp(Configuration);

            // Configure API
            services.ConfigureMvc();
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.ConfigureOpenApiGeneration();
            services.ConfigureRouting();
            services.ConfigureInvalidStateHandling();
            services.AddSingleton<ITelemetryClientFactory, TelemetryClientFactory>();
            services.AddSingleton<IVaultService>(new VaultService("ticketingsecure"));
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
            app.UseExceptionHandlerWithProblemJson();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseMvc();
            app.UseOpenApi();

            // Configure Automapper
            AutoMapperConfig.Initialize();
        }
    }
}
