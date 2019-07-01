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
using Newtonsoft.Json;
using System;
using WenceyWang.FIGlet;

namespace FaaS.Ticketing.Email
{
    public class fx_sendgrid
    {
        private IConfiguration _configuration;

        public fx_sendgrid(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("fx_sendgrid")]
        [return: SendGrid(ApiKey = "SendGrid-ApiKey", From = "SendGrid-FromRecipient")]
        public SendGridMessage Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            var text = new AsciiArt("to sendgrid");
            log.LogInformation(text.ToString());

            log.LogInformation($"C# Queue trigger function processed order: {eventGridEvent.Data}");

            var emailFrom = Environment.GetEnvironmentVariable("SendGrid-FromRecipient");
            var jsonData = JsonConvert.SerializeObject(eventGridEvent.Data);
            var orderData = JsonConvert.DeserializeObject<Order>(jsonData);
            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Thanks for your order # {orderData.OrderId})"
            };
            message.AddContent("text/plain", $"{orderData.Recipient}, your order ({orderData.OrderId}) is being processed!");
            message.SetFrom(emailFrom);
            message.AddTo("massimo.crippa@gmail.com");
            
            return message;
        }
    }
    public class Order
    {
        public string OrderId { get; set; }
        public string Recipient { get; set; }
        public string Source { get; set; }
    }
}
