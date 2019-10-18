using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaaS.Ticketing.ApiLib.DTOs;
using PaaS.Ticketing.ApiLib.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using IdentityProviderNs = Ticketing.Idp;

namespace PaaS.Ticketing.Api.IntegationTest
{
    [Collection("TestServer")]
    public class ConcertTest
    {
        TestServerFixture fixture;

        public ConcertTest(TestServerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetConcerts_Ok_TestAsync()
        {
            //Arrange
            var request = new HttpRequestMessage(new HttpMethod("GET"), "/core/v1/concerts");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            ////Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PlaceNewOrder_TestAsync()
        {

            //Arrange
            var userGuid = Guid.NewGuid().ToString();
            var newUser = new UserCreateDto
            {
                Firstname = String.Format("user{0}", userGuid),
                Lastname = "Test user",
                Phone = "000001111",
                Email = "test@user.com"
            };
            var requestUser = TestExtensions.GetJsonRequest(newUser, "POST", $"/core/v1/users/");
            var responseUser = await fixture._httpClient.SendAsync(requestUser);
            responseUser.StatusCode.Should().Be(HttpStatusCode.Created);

            var obj = JsonConvert.DeserializeObject<User>(responseUser.Content.ReadAsStringAsync().Result);
            var newOrder = new OrderCreateDto
            {
                UserId = obj.UserId,
                ConcertId = new Guid("6df7cee9-7081-41ce-947b-c305a33a3888"),
                TicketDate = DateTime.Now
            };
            var request = TestExtensions.GetJsonRequest(newOrder, "POST", $"/core/v1/orders/");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetOrder_WithJwtToken_Ok_TestAsync()
        {
            //Arrange
            var idpServer = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<IdentityProviderNs.Startup>());
            var idpClient = idpServer.CreateClient();
            idpClient.BaseAddress = new Uri("https://localhost:44351");

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("client_id", "ticketingtestapp"));
            keyValues.Add(new KeyValuePair<string, string>("client_secret", "secret"));
            keyValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            keyValues.Add(new KeyValuePair<string, string>("scope", "api://ticketing-core"));

            var idpContent = new FormUrlEncodedContent(keyValues);
            idpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var idpRequest = new HttpRequestMessage(new HttpMethod("POST"), "/connect/token");
            idpRequest.Content = idpContent;
            //Act
            var idpResponse = await idpClient.SendAsync(idpRequest);
            //Assert
            idpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var idpToken = JObject.Parse(idpResponse.Content.ReadAsStringAsync().Result);
            var accessToken = idpToken["access_token"].ToString();

            //Arrange
            var request = new HttpRequestMessage(new HttpMethod("GET"), "/core/v1/orders/links/67890");
            fixture._httpClient.DefaultRequestHeaders.Accept.Clear();
            fixture._httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            fixture._httpClient.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue("Bearer", accessToken);
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            ////Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
