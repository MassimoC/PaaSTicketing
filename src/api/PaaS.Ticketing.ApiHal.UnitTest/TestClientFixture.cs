using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Xunit;


namespace PaaS.Ticketing.ApiHal.UnitTest
{
    public class TestClientFixture
    {
        internal readonly HttpClient _httpClient;

        public TestClientFixture()
        {
            if (_httpClient != null) return;
            if (System.Environment.GetEnvironmentVariable("Security__VaultName") == null) LaunchSettingsWorkaround();


            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri("https://localhost:44311");
        }

        public static void LaunchSettingsWorkaround()
        {
            const string launchSettingsJson = @"unittestparameters.json";
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

    [CollectionDefinition("TestClient")]
    public class TestClientCollection : ICollectionFixture<TestClientFixture>
    {
    }
}
