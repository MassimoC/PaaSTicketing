using FluentAssertions;
using PaaS.Ticketing.ApiLib.DTOs;
using PaaS.Ticketing.ApiLib.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PaaS.Ticketing.Api.IntegationTest
{
    [Collection("TestServer")]
    public class HttpStatusCodesTest
    {
        TestServerFixture fixture;

        public HttpStatusCodesTest(TestServerFixture fixture)
        {
            this.fixture = fixture;
            fixture._httpClient.DefaultRequestHeaders.Remove("Accept");
        }

        [Fact]
        public async Task ProblemJson_404_RouteNotExists_Test()
        {
            //Arrange
            var request = new HttpRequestMessage(new HttpMethod("GET"), "/core/v1/concert");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Content.Headers.ContentLength.Should().BeGreaterThan(0);
            response.ShouldBeProblemJson();
        }

        [Fact]
        public async Task ProblemJson_405_MethodNotAllowed_Test()
        {
            //Arrange
            var concert = new Concert
            {
                ConcertId = Guid.NewGuid(),
                Name = "Best Kept Secret Festival 2019"
            };
            var request = TestExtensions.GetJsonRequest(concert, "PUT", "/core/v1/concerts");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            response.Content.Headers.ContentLength.Should().BeGreaterThan(0);
            response.ShouldBeProblemJson();
        }

        [Fact]
        public async Task ProblemJson_400_ModelValidation_Test()
        {
            //Arrange
            var concert = new User
            {
                UserId = Guid.NewGuid()
            };
            var request = TestExtensions.GetJsonRequest(concert, "POST", "/core/v1/users");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentLength.Should().BeGreaterThan(0);
            response.ShouldBeProblemJson();
        }

        [Fact]
        public async Task ProblemJson_406_NotAcceptable_Test()
        {
            //Arrange
            var request = new HttpRequestMessage(new HttpMethod("GET"), "/core/v/users");
            fixture._httpClient.DefaultRequestHeaders.Add("Accept", "custom/content+type");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
            response.Content.Headers.Should().BeNullOrEmpty(); // With 406 the body is suppressed
        }

        [Fact]
        public async Task ProblemJson_415_UnsupportedContentType_Test()
        {
            //Arrange
            var user = new UserCreateDto
            {
                Firstname = "Michelle",
                Lastname = "Obama",
                Email = "michelle.thegreatest@whatever.com",
                Phone = "003255945521"
            };
            var request = TestExtensions.GetJsonRequest(user, "POST", "/core/v1/users", "application/pdf");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            response.Content.Headers.ContentLength.Should().BeGreaterThan(0);
            response.ShouldBeProblemJson();
        }

        [Fact]
        public async Task ProblemJson_500_InternalServerError_Test()
        {
            //Arrange
            var uid = "00000000-0000-0000-0000-000000000000";
            var request = new HttpRequestMessage(new HttpMethod("GET"), $"/core/v1/users/{uid}");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            response.Content.Headers.ContentLength.Should().BeGreaterThan(0);
            response.ShouldBeProblemJson();
        }
    }
}
