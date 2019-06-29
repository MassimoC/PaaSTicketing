using Arcus.Security.Providers.AzureKeyVault.Authentication;
using Arcus.Security.Providers.AzureKeyVault.Configuration;
using Arcus.Security.Secrets.AzureKeyVault;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Security
{

    public class VaultService : IVaultService
    {
        private ManagedServiceIdentityAuthenticator _vaultAuthenticator;
        private KeyVaultConfiguration _vaultConfiguration;
        private KeyVaultSecretProvider _keyVaultSecretProvider;
        private readonly IConfiguration _configuration;

        public VaultService(IConfiguration configuration) 
        {
            _configuration = configuration;
            var vaultName = _configuration.GetSection("Security:VaultName").Value;
            if (!String.IsNullOrEmpty(vaultName))
            {
                Init(vaultName);
            }
        }
        public VaultService(string vaultName)
        {
            Init(vaultName);
        }

        private void Init(string vaultName)
        {
            _vaultAuthenticator = new ManagedServiceIdentityAuthenticator();
            _vaultConfiguration = new KeyVaultConfiguration($"https://{vaultName}.vault.azure.net/");
            _keyVaultSecretProvider = new KeyVaultSecretProvider(_vaultAuthenticator, _vaultConfiguration);
        }

        public async Task<string> GetSecret(string secretName)
        {
            return await _keyVaultSecretProvider.Get(secretName);
        }
    }

}
