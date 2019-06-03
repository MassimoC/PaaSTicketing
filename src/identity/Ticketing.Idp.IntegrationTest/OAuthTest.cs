using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ticketing.Idp.IntegrationTest
{
    public class OAuthTest
    {
        private readonly HttpClient _httpClient;
        public OAuthTest()
        {
            if (_httpClient != null) return;
            var srv = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Ticketing.Idp.Startup>());

            _httpClient = srv.CreateClient();
        }

        [Fact]
        public async Task GetToken_ClientCredentials()
        {
            //Arrange
            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("client_id", "ticketingtestapp"));
            keyValues.Add(new KeyValuePair<string, string>("client_secret", "secret"));
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            keyValues.Add(new KeyValuePair<string, string>("scope", "api://ticketing-core"));

            var content = new FormUrlEncodedContent(keyValues);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var request = new HttpRequestMessage(new HttpMethod("POST"), "/connect/token");
            request.Content = content;

            //Act
            var response = await _httpClient.SendAsync(request);
            
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var idpResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var accessToken = idpResponse["access_token"].ToString();
            accessToken.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadToken(accessToken) as JwtSecurityToken;

            // Audience should be the API and not the user
            jwtSecurityToken.Audiences.Should().Contain("api://ticketing-core");
            jwtSecurityToken.Issuer.Should().Contain("localhost");
        }

        [Fact]
        public async Task GetToken_ResourceOwner()
        {
            //Arrange
            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("username", "Bob"));
            keyValues.Add(new KeyValuePair<string, string>("password", "Mallo"));
            keyValues.Add(new KeyValuePair<string, string>("client_id", "ticketingtestapp"));
            keyValues.Add(new KeyValuePair<string, string>("client_secret", "secret"));
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));
            keyValues.Add(new KeyValuePair<string, string>("scope", "api://ticketing-core"));

            var content = new FormUrlEncodedContent(keyValues);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var request = new HttpRequestMessage(new HttpMethod("POST"), "/connect/token");
            request.Content = content;

            //Act
            var response = await _httpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var idpResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var accessToken = idpResponse["access_token"].ToString();
            accessToken.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadToken(accessToken) as JwtSecurityToken;

            // Audience should be the API and not the user
            jwtSecurityToken.Audiences.Should().Contain("api://ticketing-core");
            jwtSecurityToken.Issuer.Should().Contain("localhost");
        }

        public static HttpRequestMessage GetJsonRequest(object data, string method, string requestUri, string contentType = MediaTypeNames.Application.Json)
        {
            var serializedData = JsonConvert.SerializeObject(data);
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(serializedData));
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var request = new HttpRequestMessage(new HttpMethod(method), requestUri);
            request.Content = content;

            return request;
        }
    }
}
