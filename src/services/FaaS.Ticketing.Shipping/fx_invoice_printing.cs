using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.IO;
using System.Text;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using PaaS.Ticketing.Security;
using WenceyWang.FIGlet;

namespace FaaS.Ticketing.Shipping
{
    public static class fx_invoice_printing
    {
        [FunctionName("fx_invoice_printing")]
        public static async Task<string> PrintInvoiceAsync([ActivityTrigger] DurableActivityContext inputs, ILogger log)
        {
            CloudStorageAccount cloudStorageAccount = null;

            var (orderId, parentId) = inputs.GetInput<(string, string)>();
            log.LogInformation($" >>>>>> Printing invoice for the order {orderId}. <<<<<<");

            var storageConnectionString = String.Empty;
            // local debug uses visualstudio user as MSI
            var securityVault = new VaultService(Environment.GetEnvironmentVariable("VaultName"));
            try
            {
                storageConnectionString = securityVault.GetSecret("cn-storageaccount").Result;
            }
            catch (Exception)
            {
                storageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            }

            var text = new AsciiArt("to blob storage");
            log.LogInformation(text.ToString());

            // save document to blob storage
            cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var blobContainer = cloudBlobClient.GetContainerReference("invoices");

            var blockBlob = blobContainer.GetBlockBlobReference($"{orderId}.png");
            var bytes = Encoding.ASCII.GetBytes(orderId);
            using (var ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Flush();
                ms.Position = 0;
                blockBlob.UploadFromStream(ms);
            }
   
            // Demo code : Never use random in functions
            return String.Format("LBL{0}",Guid.NewGuid().ToString());
        }

        public static async Task<string> GetSecretFromVault(string vaultName, string secretName, ILogger log)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyvaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secretValue = await keyvaultClient.GetSecretAsync($"https://{vaultName}.vault.azure.net/", secretName);
            return secretValue.Value;
        }

    }
}
