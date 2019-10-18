using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Diagnostics;

namespace PaaS.Ticketing.ApiLib.Extensions
{
    public class ProblemDetailsError : ProblemDetails
    {
        public ProblemDetailsError(int statusCode, string detail = "", string cid ="") : base()
        {
            var instanceId = (!String.IsNullOrEmpty(cid)) ? cid : Activity.Current.Id;

            var errorType = statusCode.Between(500, 599, true) ? Constants.API.ServerError :
                (statusCode.Between(400, 499, true) ? Constants.API.ClientError : "unknown");

            this.Status = statusCode;
            this.Title = ReasonPhrases.GetReasonPhrase(statusCode);
            if (!String.IsNullOrEmpty(detail)) { this.Detail = detail; }
            this.Instance = $"urn:{Constants.API.CompanyName}:{errorType}:{instanceId}";
        }

        public ProblemDetailsError() : base()
        { }

    }
}
