using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FaaS.Ticketing.Shipping
{
    public static class fx_package_labeling
    {
        [FunctionName("fx_package_labeling")]
        public static string GenerateLabel([ActivityTrigger] string orderId, ILogger log)
        {
            log.LogInformation($" >>>>>> Package composition: order {orderId} <<<<<<");

            var topicEndpoint = Environment.GetEnvironmentVariable("TopicEndpoint");
            var topicKey = Environment.GetEnvironmentVariable("TopicKey");

            string topicHostname = new Uri(topicEndpoint).Host;
            using (EventGridClient client = new EventGridClient(new TopicCredentials(topicKey)))
            {
                client.PublishEventsAsync(topicHostname, GetEvent(orderId)).GetAwaiter().GetResult();
            }

            // very BAD demo code : Never use random in durable function
            return String.Format("PKG{0}", Guid.NewGuid().ToString());
        }

        static IList<EventGridEvent> GetEvent(string id)
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
                Subject = $"/ticketing/orders/{id}",
                DataVersion = "1.0"
            });

            return eventsList;
        }
    }
}