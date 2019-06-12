using CloudNative.CloudEvents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaaS.Ticketing.Events;
using PaaS.Ticketing.Events.Data;
using Polly;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FaaS.Ticketing.NotificationHandler
{
    public static class fx_update_status
    {
        // TODO : use HttpClientFactory
        private static HttpClient _httpClient = new HttpClient();

        [FunctionName("fx_update_status")]
        public static async Task Run(
            [ServiceBusTrigger("q-notifications", Connection = "SB-Queue-In-AppSettings")]
            string notificationMsg,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Incoming message from queue q-notifications");
            // TODO: This should be done with a DURABLE function
            // or implement compensation

            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var sbConnectionString = config["Values:SB-Queue-In-AppSettings"];

            var jsonFormatter = new JsonEventFormatter();
            var inboundEvent = jsonFormatter.DecodeStructuredEvent(Encoding.UTF8.GetBytes(notificationMsg));
            log.LogInformation($" message type : { inboundEvent.Type}");

            var notification = JsonConvert.DeserializeObject<ObjectStatus>((string)inboundEvent.Data);
            dynamic jPatch = new JObject();
            jPatch.op= "replace";
            jPatch.path = "/status";
            jPatch.value = notification.Status;
            var patchArray = new JArray(jPatch);
            log.LogInformation($" Status to be changed for the object {notification.Id}  value: { notification.Status}");

            var httpContent = new StringContent(patchArray.ToString(), Encoding.UTF8, "application/json-patch+json");
            // TODO parametrization missing
            var url = string.Format("https://localhost:44328/core/v1/orders/{0}", notification.Id);
            //TODO add authentication
            //_httpClient.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue("Bearer", "to be added");

            log.LogInformation(" Call the web api ...");
            var response = await Policy
                .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5)
                }, (result, timeSpan, retryCount, ctx) =>
                {
                    log.LogWarning($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Attempt #{retryCount}");
                })
                .ExecuteAsync(() => _httpClient.PatchAsync(url, httpContent));

            if (!response.IsSuccessStatusCode)
            {
                var pub = new Publisher("q-errors", sbConnectionString);
                var result = pub.SendMessagesAsync(notificationMsg);
            }

            return;
        }
    }
}
