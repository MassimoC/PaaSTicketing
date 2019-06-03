using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace PaaS.Ticketing.Api.IntegationTest
{
    public class TestServerFixture
    {
        internal readonly HttpClient _httpClient;

        public TestServerFixture()
        {
            if (_httpClient != null) return;
            var srv = new TestServer(new WebHostBuilder()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment;

                    config.SetBasePath(env.ContentRootPath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .UseEnvironment("Development")
                .UseUrls("https://*:44326") 
                .UseIISIntegration()
                .UseStartup<Startup>());

            _httpClient = srv.CreateClient();
            _httpClient.BaseAddress = new System.Uri("https://localhost:44326");
        }
    }

    [CollectionDefinition("TestServer")]
    public class TestServerCollection : ICollectionFixture<TestServerFixture>
    {
    }
}
