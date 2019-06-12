using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Text;
using CloudNative.CloudEvents;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaaS.Ticketing.Events.Data;

namespace FaaS.Ticketing.Shipping
{
    public static class fx_complete_shipping
    {
        private static TelemetryClient _telemetryClient = new TelemetryClient(TelemetryConfiguration.Active);

        [FunctionName("fx_complete_shipping")]
        [return: ServiceBus("q-notifications", Connection = "SB-Queue-In-AppSettings")]
        public static string SaveToQueue([ActivityTrigger] DurableActivityContext inputs, ILogger log)
        {
            string notificationMsg;
            var (orderId, parentId) = inputs.GetInput<(string, string)>();

            // dependency tracking
            var requestActivity = new Activity("command://order.update");
            requestActivity.SetParentId(parentId);
            var requestOperation = _telemetryClient.StartOperation<RequestTelemetry>(requestActivity);

            try
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
                notificationMsg = Encoding.UTF8.GetString(messageBody);
            }
            catch (Exception ex)
            {
                // dependency tracking
                _telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                // dependency tracking
                _telemetryClient.StopOperation(requestOperation);
            }
            return notificationMsg;
        }
    }
}