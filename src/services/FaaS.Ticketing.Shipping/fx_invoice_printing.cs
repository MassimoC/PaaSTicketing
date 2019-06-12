using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FaaS.Ticketing.Shipping
{
    public static class fx_invoice_printing
    {
        [FunctionName("fx_invoice_printing")]
        public static async Task<string> PrintInvoiceAsync([ActivityTrigger] DurableActivityContext inputs, ILogger log)
        {
            var (orderId, parentId) = inputs.GetInput<(string, string)>();

            log.LogInformation($" >>>>>> Printing invoice for the order {orderId}. <<<<<<");
            // TODO save document to blob storage
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Demo code : Never use random in functions
            return String.Format("LBL{0}",Guid.NewGuid().ToString());
        }
    }
}
