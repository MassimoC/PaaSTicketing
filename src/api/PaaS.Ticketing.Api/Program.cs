using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaaS.Ticketing.ApiLib.Context;
using System;

namespace PaaS.Ticketing.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            // db migrations
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetService<TicketingContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment;

                    config.SetBasePath(env.ContentRootPath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((context, builder) =>
                    {
                        builder.ClearProviders();
                        builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                        builder.AddApplicationInsights(context.Configuration["ApplicationInsights:InstrumentationKey"]);
                        builder.AddConsole(options => options.IncludeScopes = true);
                        builder.AddDebug();
                    }
                )
                .UseStartup<Startup>();
    }
}
