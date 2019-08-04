using Microsoft.AspNetCore.Http;
using PaaS.Ticketing.ApiLib.Extensions;
using Swashbuckle.AspNetCore.Filters;

namespace PaaS.Ticketing.Api.Examples
{
    public class DocProblemDetail404 : IExamplesProvider
    {
        public object GetExamples()
        {
            return new ProblemDetailsError(StatusCodes.Status404NotFound,"", "|32characters00000000000000000000.8chars00_");
        }
    }
}
