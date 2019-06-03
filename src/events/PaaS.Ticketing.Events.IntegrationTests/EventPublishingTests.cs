using Arcus.EventGrid.Testing.Infrastructure.Hosts;
using Arcus.EventGrid.Testing.Infrastructure.Hosts.ServiceBus;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace PaaS.Ticketing.Events.IntegrationTests
{
    public class EventPublishingTests : IAsyncLifetime
    {
        private HybridConnectionHost _hybridConnectionHost;
        private ServiceBusEventConsumerHost _serviceBusEventConsumerHost;

        protected IConfiguration Configuration { get; }

        public async Task DisposeAsync()
        {
            await _hybridConnectionHost.Stop();
        }

        public async Task InitializeAsync()
        {
            var relayNamespace = "none.servicebus.windows.net";
            var hybridConnectionName = "none";
            var accessPolicyName = "none";
            var accessPolicyKey = "none";
            
            ILogger logger = new DebugLogger("Test");

            _hybridConnectionHost = await HybridConnectionHost.Start(relayNamespace, hybridConnectionName, accessPolicyName, accessPolicyKey, logger);
        }


        [Fact]
        public async Task Publish_ValidParameters_Succeeds()
        {
            // Arrange
            var topicEndpoint = "https://none.westeurope-1.eventgrid.azure.net/api/events";
            var topicKey = "none";
            var eventId = Guid.NewGuid().ToString();

            // Act
            string topicHostname = new Uri(topicEndpoint).Host;
            TopicCredentials topicCredentials = new TopicCredentials(topicKey);
            EventGridClient client = new EventGridClient(topicCredentials);
            //client.PublishEventsAsync(topicHostname, GetEvents()).GetAwaiter().GetResult();
            await client.PublishEventsAsync(topicHostname, GetEvents("12345678"));

            // Assert
            var receivedEvent = _hybridConnectionHost.GetReceivedEvent(eventId, new TimeSpan(0, 0, 10));
            System.Diagnostics.Debug.WriteLine(receivedEvent);
            Assert.NotEmpty(receivedEvent);
        }

        static IList<EventGridEvent> GetEvents(string id)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>();
            dynamic eventData = new ExpandoObject();
            eventData.OrderId = id;
            eventData.Recipient = "massimo.crippa@gmail.com";
            eventData.Source = new Uri("app://ticketing.services.labeling");

            eventsList.Add(new EventGridEvent()
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "event://order.completed",
                Data = eventData,
                EventTime = DateTime.Now,
                Subject = "/ticketing/orders/12345678",
                DataVersion = "1.0"
            });

            return eventsList;
        }
    }
}
