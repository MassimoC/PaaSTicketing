using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Security
{
    public interface IVaultService
    {
        Task<string> GetSecret(string secretName);
    }
}
