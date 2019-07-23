using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;

namespace PaaS.Ticketing.ApiLib.Extensions
{
    /// <summary>
    /// Telemetry Client extensions
    /// </summary>
    public static class TelemetryClientExtensions
    {
        public static DependencyTelemetry StartExternalDependencyTelemetry(this TelemetryClient telemetryClient, 
            in string parentId, 
            in string methodName, 
            ValueTuple<string, string>[] args)
        {
            DependencyTelemetry dependencyTelemetry = new DependencyTelemetry
            {
                Name = methodName
            };
            dependencyTelemetry.Context.Operation.ParentId = parentId;

            for (int i = 0; i < args.Length; i++)
            {
                dependencyTelemetry.Properties.Add(args[i].Item1, args[i].Item2);
            }
            dependencyTelemetry.Start();
            return dependencyTelemetry;
        }
    }
}
