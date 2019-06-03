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

namespace FaaS.Ticketing.Email
{
    public static class fx_sendgrid_svc
    {
        [FunctionName("fx_sendgrid_svc")]
        [return: SendGrid(ApiKey = "SendGrid-ApiKey", From = "SendGrid-FromRecipient")]
        public static SendGridMessage Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed order: {eventGridEvent.Data}");

            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Thanks for your order (#{eventGridEvent.Data})!"
            };

            message.AddTo("massimo.crippa@gmai.com");
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
