using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaaS.Ticketing.Events.Data
{
    public class ObjectStatus
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }
    }
}
