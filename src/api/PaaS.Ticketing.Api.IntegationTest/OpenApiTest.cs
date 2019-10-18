using FluentAssertions;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PaaS.Ticketing.Api.IntegationTest
{
    [Collection("TestServer")]
    public class OpenApiTest
    {
        TestServerFixture fixture;

        public OpenApiTest(TestServerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task OpenApi_OperationId_Test()
        {
            //Arrange
            var versionId = "v1";
            var request = new HttpRequestMessage(new HttpMethod("GET"), "/swagger/v1/swagger.json");
            //Act
            var response = await fixture._httpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var swaggerDoc = JObject.Parse(await response.Content.ReadAsStringAsync());
            var paths = (JObject)swaggerDoc.SelectToken("paths");
            paths.Count.Should().Be(9);

            foreach (var item in paths)
            {
                item.Key.Should().Contain(versionId);
                ValidateOperationId(HttpMethod.Get, item);
                ValidateOperationId(HttpMethod.Post, item);
                ValidateOperationId(HttpMethod.Put, item);
                ValidateOperationId(HttpMethod.Patch, item);
                ValidateOperationId(HttpMethod.Delete, item);
            }
        }

        [Fact]
        public async Task OpenApi_DocumentValidation_Test()
        {
            //Arrange        
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://fileingestion.blob.core.windows.net/")
            };
            var stream = await httpClient.GetStreamAsync("temp/swagger.json");

            //Act
            var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);
            //Assert
            openApiDocument.Paths.Count.Should().Be(9);
        }

        private void ValidateOperationId(HttpMethod httpMethod, KeyValuePair<string, JToken> item)
        {
            var operationId = String.Format("{0}.operationId", httpMethod.ToString().ToLowerInvariant());
            var openApiToken = ((string)item.Value.SelectToken(operationId));
            if (!string.IsNullOrEmpty(openApiToken)) openApiToken.Should().Contain("_");
        }
    }
}
