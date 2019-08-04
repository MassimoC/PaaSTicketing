using Microsoft.AspNetCore.Http;
using PaaS.Ticketing.ApiLib.Extensions;
using Swashbuckle.AspNetCore.Filters;

namespace PaaS.Ticketing.Api.Examples
{
    public class DocProblemDetail500 : IExamplesProvider
    {
        public object GetExamples()
        {
            return new ProblemDetailsError(StatusCodes.Status500InternalServerError, "System.ArgumentException: Value not supported\n   at async Task<IActionResult> ...", "|32characters00000000000000000000.8chars00_");
        }
    }
}
