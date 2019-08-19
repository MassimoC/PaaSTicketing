using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PaaS.Ticketing.ApiHal.UnitTest
{
    [Collection("TestClient")]
    public class ConcertTest
    {
        TestClientFixture fixture;

        public ConcertTest(TestClientFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetConcerts_Ok_TestAsync()
        {
            //Arrange
            var request = new HttpRequestMessage(new HttpMethod("GET"), "/hal/v1/concerts");
            //Act
            var response = await fixture._httpClient.SendAsync(request);
            ////Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }      
    }
}
