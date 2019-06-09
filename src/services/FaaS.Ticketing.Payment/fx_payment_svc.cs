using CloudNative.CloudEvents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaaS.Ticketing.Events;
using PaaS.Ticketing.Events.Data;
using System;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;
using LoggingContext = PaaS.Ticketing.Events.Logging.Constants;

namespace FaaS.Ticketing.Payment
{
    public static class fx_payment_svc
    {
        [FunctionName("fx_payment_svc")]
        public static void Run(
            [ServiceBusTrigger("q-payment-in", Connection = "SB-Queue-In-AppSettings")]
            string paymentItem,
            [ServiceBus("q-notifications", Connection = "SB-Queue-In-AppSettings")] out string notificationEventString,
            [ServiceBus("q-shipping-in", Connection = "SB-Queue-In-AppSettings")] out string outboundEventString,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("start fx_payment_svc function");
            outboundEventString = null;
            notificationEventString = null;

            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var sbConnectionString = config["Values:SB-Queue-In-AppSettings"];

            try
            {

                ContentType contentType;
                var jsonFormatter = new JsonEventFormatter();
                var inboundEvent = jsonFormatter.DecodeStructuredEvent(Encoding.UTF8.GetBytes(paymentItem));
                log.LogInformation($" message type : { inboundEvent.Type}");

                // TODO: This should be done with a DURABLE function
                // or implement compensation

                // 1. perform the payment
                var paymentCtx = JsonConvert.DeserializeObject<PaymentContext>((string)inboundEvent.Data);
                Pay(paymentCtx);
                log.LogInformation("Payment is done for {paymentCtx.Attendee}. OrderId : {paymentCtx.OrderId} - Token {paymentCtx.Token}");

                log.LogInformation(new EventId((int)LoggingContext.EventId.Processing),
                                      LoggingContext.Template,
                                      "cloud event publishing [command://order.pay]",
                                      LoggingContext.EntityType.Order.ToString(),
                                      LoggingContext.EventId.Processing.ToString(),
                                      LoggingContext.Status.Pending.ToString(),
                                      "correlationId",
                                      LoggingContext.CheckPoint.Publisher.ToString(),
                                      "long description");

                // 2. notify the change
                var notificationEvent = new CloudEvent(
                    "command://order.update",
                    new Uri("app://ticketing.services.payment"))
                {
                    Id = Guid.NewGuid().ToString(),
                    ContentType = new ContentType(MediaTypeNames.Application.Json),
                    Data = JsonConvert.SerializeObject(new ObjectStatus()
                    {
                        Id = paymentCtx.OrderId,
                        Status = "Paid"
                    })
                };
                // return as string to avoid ContentType deserialization problem
                var messageBody = jsonFormatter.EncodeStructuredEvent(notificationEvent, out contentType);
                notificationEventString = Encoding.UTF8.GetString(messageBody);

                // 3. trigger next step
                var outboundEvent = new CloudEvent(
                    "command://order.deliver",
                    new Uri("app://ticketing.services.payment"))
                    {
                    Id = Guid.NewGuid().ToString(),
                    ContentType = new ContentType(MediaTypeNames.Application.Json),
                    Data = JsonConvert.SerializeObject(paymentCtx)
                };
                messageBody = jsonFormatter.EncodeStructuredEvent(outboundEvent, out contentType);
                outboundEventString = Encoding.UTF8.GetString(messageBody);

            }
            catch (System.Exception ex)
            {
                // TODO : compensation
                var pub = new Publisher("q-errors", sbConnectionString);
                var result = pub.SendMessagesAsync(paymentItem);
                log.LogError(ex.Message);
            }

            log.LogInformation("end function");
        }

        private static void Pay(PaymentContext paymentCtx)
        {
            return;
        }
    }
}