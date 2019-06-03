using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;

namespace PaaS.Ticketing.Api.Factories
{
    public class TelemetryClientFactory : ITelemetryClientFactory
    {
        private readonly IConfiguration _configuration;

        public TelemetryClientFactory(IConfiguration apiConfiguration)
        {
            _configuration = apiConfiguration;
        }

        public TelemetryClient Create()
        {
            return new TelemetryClient()
            {
                InstrumentationKey = _configuration.GetSection("ApplicationInsights:InstrumentationKey").Value
            };
        }

    }
}
