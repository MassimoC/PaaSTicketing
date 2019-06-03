﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api
{
    public static class Constants
    {
        public static class Messages
        {
            public const string ProblemDetailsDetail = "Please refer to the errors property for additional details.";
        }

        public static class OpenApi
        {
            public const string Title = "Ticketing API";
            public const string Description = "PaaS Ticketing System";
            public const string TermsOfService = "N/A";
            public const string ContactName = "API at Ticketing";
            public const string ContactEmail = "support@ticketing.com";
            public const string ContactUrl = "https://www.ticketing.com";
        }
    }
}
