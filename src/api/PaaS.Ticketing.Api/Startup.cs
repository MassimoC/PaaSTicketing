using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaaS.Ticketing.Api.Extensions;
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
            services.ConfigureAI();
            _logger.LogInformation("Startup - Configuring CORE services...");
            
            // Add functionality to inject IOptions<T>
            services.AddOptions();
            services.AddSingleton<IVaultService>(new VaultService(_configuration["Security:VaultName"]));
            
            // for migrations
            //var cn = Configuration.GetConnectionString(name: "TicketingDB");
            //services.AddDbContext<TicketingContext>(o => o.UseSqlServer(cn));
            services.ConfigureDatabase(_configuration);
            services.ConfigureIdp(_configuration);
           
            // Configure API
            services.ConfigureMvc();
            services.ConfigureOpenApiGeneration(true);
            services.ConfigureOpenApiExamples();
            services.ConfigureRouting();
            services.ConfigureInvalidStateHandling();
        }

        /// <summary>
        /// // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="env">hosting environment</param>
        /// <param name="concertContext">db context</param>
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

            app.UseHttpsRedirection();
            // prefer the middleware approach
            //app.UseExceptionHandlerWithProblemJson();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            //app.UseMiddleware<ScopedLoggingMiddleware>();
            app.UseAuthentication();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseMvc();
            app.UseOpenApi();
            AutoMapperConfig.Initialize();
        }
    }
}
