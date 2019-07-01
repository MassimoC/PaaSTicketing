using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;


[assembly: FunctionsStartup(typeof(FaaS.Ticketing.Email.Startup))]

namespace FaaS.Ticketing.Email
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                       .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                       .AddEnvironmentVariables()
                       .Build();

            builder.Services.AddSingleton(config);
            //builder.Services.AddHttpClient();
            //builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }
    }
}


