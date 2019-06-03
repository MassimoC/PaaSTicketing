using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.DTOs
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public String EventName{ get; set; }
        public String Attendee { get; set; }
        public DateTime TicketDate { get; set; }
        public String Status { get; set; }
        public String Token { get; set; }
    }
}
