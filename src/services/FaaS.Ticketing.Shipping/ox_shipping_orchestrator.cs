using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaaS.Ticketing.Events.Data;

namespace FaaS.Ticketing.Shipping
{
    public static class ox_shipping_orchestrator
    {

        /// <summary>
        /// Orchestrator Client
        /// Responsible for starting and stopping orchestrator and monitoring. 
        /// </summary>
        /// <param name="message">Inbound message</param>
        /// <param name="starter">Client</param>
        /// <param name="log">Logger</param>
        /// <returns></returns>
        [FunctionName("ox_shipping_orchestrator_SbQueueStart")]
        public static async Task SBQueueStart(
            [ServiceBusTrigger("q-shipping-in", Connection = "SB-Queue-In-AppSettings")] string paymentItem,
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {

            var jsonFormatter = new JsonEventFormatter();
            var inboundEvent = jsonFormatter.DecodeStructuredEvent(Encoding.UTF8.GetBytes(paymentItem));
            var paymentCtx = JsonConvert.DeserializeObject<PaymentContext>((string)inboundEvent.Data);
            log.LogInformation($" message type : { inboundEvent.Type}");
            log.LogInformation($"Processing shipment for OrderId : {paymentCtx.OrderId} - Token {paymentCtx.Token}");

            string instanceId = await starter.StartNewAsync("ox_shipping_orchestrator", paymentCtx.OrderId);
            log.LogInformation($"Started orchestration with ID = '{instanceId}' OrderId '{paymentCtx.OrderId}'.");

            return;
        }

        /// <summary>
        /// The orchestrator (the workflow, if u prefer)
        /// Calls activities or other orchestrators or waits for events to get triggered
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [FunctionName("ox_shipping_orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            List<string> output = null;
            try
            {
                string orderId = context.GetInput<string>();

                // TODO parallel
                var invoiceId = await context.CallActivityAsync<string>("fx_invoice_printing", orderId);
                var labelId = await context.CallActivityAsync<string>("fx_package_labeling", orderId);
                var notification = await context.CallActivityAsync<CloudEvent>("fx_complete_shipping", orderId);

                // FAN-OUT
                //Task<string> invoiceId = await context.CallActivityAsync<Task<string>>("fx_invoice_printing", orderId);
                //Task<string> labelId = await context.CallActivityAsync<Task<string>>("fx_package_labeling", orderId);
                //await Task.WhenAll(invoiceId, labelId);
                               
            }
            catch (System.Exception)
            {
                // compensation block
                throw;
            }
            return output;
        }
    }
}