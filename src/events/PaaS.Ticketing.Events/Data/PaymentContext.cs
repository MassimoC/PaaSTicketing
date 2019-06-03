using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaaS.Ticketing.Events.Data
{
    public class PaymentContext
    {
        [JsonProperty("attendee", Required = Required.Always)]
        public string Attendee { get; set; }

        [JsonProperty("orderId", Required = Required.Always)]
        public string OrderId { get; set; }

        [JsonProperty("token", Required = Required.Always)]
        public string Token { get; set; }
    }
}
