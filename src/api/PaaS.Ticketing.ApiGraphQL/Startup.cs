using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaaS.Ticketing.ApiGraphQL.Schemas;
using PaaS.Ticketing.ApiLib.Extensions;
using PaaS.Ticketing.ApiLib.Factories;
using PaaS.Ticketing.ApiLib.Middlewares;
using PaaS.Ticketing.Security;

namespace PaaS.Ticketing.ApiGraphQL
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
            _logger.LogInformation("Startup - Configuring services (GRAPH QL)...");

            // Configure DB
            services.ConfigureDatabase(Configuration);

            // Configure Identity Provider
            services.ConfigureIdp(Configuration);

            // Configure Graph QL API
            services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));
            services.AddScoped<UserSchema>();

            services.AddGraphQL(o => { o.ExposeExceptions = true; })
                .AddGraphTypes(ServiceLifetime.Scoped); // scan the assembly for all the graph types objects

            services.AddSingleton<ITelemetryClientFactory, TelemetryClientFactory>();
            services.AddSingleton<IVaultService>(new VaultService("ticketingsecure"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            // activate GraphQL middleware
            app.UseGraphQL<UserSchema>();
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());

            // Configure API (mvc not needed)
            app.UseMiddleware<ScopedLoggingMiddleware>();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");
        }
    }
}
