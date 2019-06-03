using System;
using System.Net.Mime;
using System.Text;
using CloudNative.CloudEvents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaaS.Ticketing.Events.Data;

namespace FaaS.Ticketing.Shipping
{
    public static class fx_complete_shipping
    {

        [FunctionName("fx_complete_shipping")]
        [return: ServiceBus("q-notifications", Connection = "SB-Queue-In-AppSettings")]
        public static string SaveToQueue([ActivityTrigger] string orderId, ILogger log)
        {
            ContentType contentType;
            log.LogInformation($"Sending notification for {orderId} to queue.");
            var notificationEvent = new CloudEvent(
                "command://order.update",
                new Uri("app://ticketing.services.shipping"))
                {
                Id = Guid.NewGuid().ToString(),
                ContentType = new ContentType(MediaTypeNames.Application.Json),
                Data = JsonConvert.SerializeObject(new ObjectStatus()
                {
                    Id = orderId,
                    Status = "Delivered"
                })
            };
            var jsonFormatter = new JsonEventFormatter();
            var messageBody = jsonFormatter.EncodeStructuredEvent(notificationEvent, out contentType);
            var notificationMsg = Encoding.UTF8.GetString(messageBody);

            return notificationMsg;
        }
    }
}