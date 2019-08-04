using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaaS.Ticketing.ApiLib
{
    public class CloudRoleInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                telemetry.Context.Cloud.RoleName = Constants.API.CloudRoleName;
                telemetry.Context.Cloud.RoleInstance = Constants.API.CloudRoleInstance;
            }
        }
    }
}
