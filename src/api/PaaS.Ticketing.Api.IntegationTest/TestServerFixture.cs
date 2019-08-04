    using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

namespace PaaS.Ticketing.Api.IntegationTest
{
    public class TestServerFixture
    {
        internal readonly HttpClient _httpClient;

        public TestServerFixture()
        {
            if (_httpClient != null) return;
            if (System.Environment.GetEnvironmentVariable("Security__VaultName") == null) LaunchSettingsWorkaround();

            var srv = new TestServer(new WebHostBuilder()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment;
                    config.SetBasePath(env.ContentRootPath);
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .UseStartup<Startup>());

            _httpClient = srv.CreateClient();
            _httpClient.BaseAddress = new System.Uri("https://localhost:44328");
        }

        public static void LaunchSettingsWorkaround()
        {
            const string launchSettingsJson = @"Properties\launchSettings.json";
            if (!File.Exists(launchSettingsJson))
            {
                return;
            }

            using (var file = File.OpenText(launchSettingsJson))
            {
                using (var reader = new JsonTextReader(file))
                {
                    var variables = JObject.Load(reader)
                        .GetValue("profiles")
                        .SelectMany(profiles => profiles.Children())
                        .SelectMany(profile => profile.Children<JProperty>())
                        .Where(prop => prop.Name == "environmentVariables")
                        .SelectMany(prop => prop.Value.Children<JProperty>())
                        .ToList();

                    foreach (var variable in variables)
                    {
                        Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                    }
                }
            }
        }
    }

    [CollectionDefinition("TestServer")]
    public class TestServerCollection : ICollectionFixture<TestServerFixture>
    {
    }
}
