using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaaS.Ticketing.ApiLib;
using PaaS.Ticketing.ApiLib.Context;
using PaaS.Ticketing.ApiLib.Extensions;
using PaaS.Ticketing.ApiLib.Middlewares;
using PaaS.Ticketing.Security;

namespace PaaS.Ticketing.Api
{
    public class Startup
    {
        private readonly ILogger _logger;
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add functionality to inject IOptions<T>
            services.AddOptions();
 
            // Secrets
             services.AddSingleton<IVaultService>(new VaultService(_configuration["Security:VaultName"]));
            
            // Application Insights config
            services.ConfigureAI();
            _logger.LogInformation("Startup - Configuring CORE services...");

            // for migrations
            //var cn = Configuration.GetConnectionString(name: "TicketingDB");
            //services.AddDbContext<TicketingContext>(o => o.UseSqlServer(cn));

            // Configure DB
            services.ConfigureDatabase(_configuration);

            // Configure Identity Provider
            services.ConfigureIdp(_configuration);

            // Configure API
            services.ConfigureMvc();
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.ConfigureOpenApiGeneration();
            services.ConfigureRouting();
            services.ConfigureInvalidStateHandling();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, TicketingContext concertContext)
        {
            _logger.LogInformation("Startup - Configuring CORE app...");

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
            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseMvc();
            app.UseOpenApi();

            // Configure Automapper
            AutoMapperConfig.Initialize();
        }
    }
}
