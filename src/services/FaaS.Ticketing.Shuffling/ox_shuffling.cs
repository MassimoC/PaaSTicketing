using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FaaS.Ticketing.Shuffling
{
    public static class ox_shuffling
    {
        [FunctionName("ox_shuffling_orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Function1_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        //[FunctionName("ox_shuffling_HttpStart")]
        //public static async Task<HttpResponseMessage> HttpStart(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
        //    [OrchestrationClient]DurableOrchestrationClient starter,
        //    ILogger log)
        //{
        //    // Function input comes from the request content.
        //    string instanceId = await starter.StartNewAsync("ox_shuffling_orchestrator", null);
        //    log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        //    return starter.CreateCheckStatusResponse(req, instanceId);
        //}

        [FunctionName("ox_shuffling_sbqueuestart")]
        public static async Task SBQueueStart(
            [ServiceBusTrigger("q-notifications", Connection = "SB-Queue-In-AppSettings")] string message,
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {
            var orderId = "aaayyy";
            string instanceId = await starter.StartNewAsync("ox_shuffling_orchestrator", orderId);
            log.LogInformation($"Started orchestration with ID = '{instanceId}' OrderId '{orderId}'.");

        }
    }
}