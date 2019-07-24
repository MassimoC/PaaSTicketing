using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib
{
    public static class Constants
    {
        public static class Messages
        {
            public const string ProblemDetailsDetail = "Please refer to the errors property for additional details.";
        }

        public static class OpenApi
        {
            public const string Version = "v1";
            public const string Title = "Ticketing API";
            public const string Description = "PaaS Ticketing System";
            public const string TermsOfService = "N/A";
            public const string ContactName = "API at Ticketing";
            public const string ContactEmail = "support@ticketing.com";
            public const string ContactUrl = "https://www.ticketing.com";
        }

        public static class API
        {
            public const string CloudRoleName = "Ticketing Core API";
            public const string CloudRoleInstance = "Ticketing Core API Instance";
            public const string VaultName = "ticketingsecure";
        }
    }
}
