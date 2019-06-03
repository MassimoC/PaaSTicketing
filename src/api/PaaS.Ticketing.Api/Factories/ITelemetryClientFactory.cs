using Microsoft.ApplicationInsights;

namespace PaaS.Ticketing.Api.Factories
{
    public interface ITelemetryClientFactory
    {
        TelemetryClient Create();
    }
}
