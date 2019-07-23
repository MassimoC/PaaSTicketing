using Microsoft.ApplicationInsights;

namespace PaaS.Ticketing.ApiLib.Factories
{
    public interface ITelemetryClientFactory
    {
        TelemetryClient Create();
    }
}
