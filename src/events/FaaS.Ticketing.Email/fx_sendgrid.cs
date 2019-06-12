// The 'From' and 'To' fields are automatically populated with the values specified by the binding settings.
//
// You can also optionally configure the default From/To addresses globally via host.config, e.g.:
//
// {
//   "sendGrid": {
//      "to": "user@host.com",
//      "from": "Azure Functions <samples@functions.com>"
//   }
// }
using Microsoft.Azure.WebJobs;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Configuration;

namespace FaaS.Ticketing.Email
{
    public static class fx_sendgrid
    {
        [FunctionName("fx_sendgrid")]
        [return: SendGrid(ApiKey = "SendGrid-ApiKey", From = "SendGrid-FromRecipient")]
        public static SendGridMessage Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function processed order: {eventGridEvent.Data}");

            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var emailFrom = config["Values:SendGrid-FromRecipient"];
            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Thanks for your order (#{eventGridEvent.Data})!"
            };

            message.SetFrom(emailFrom);
            message.AddTo("massimo.crippa@gmail.com");
            message.AddContent("text/plain","your order has been processed");
            //message.AddContent("text/plain", $"{order.CustomerName}, your order ({order.OrderId}) is being processed!");
            return message;
        }
    }
    public class Order
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
    }
}
