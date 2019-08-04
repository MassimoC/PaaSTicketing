using System;

namespace PaaS.Ticketing.ApiLib.DTOs
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public Guid EventId { get; set; }
        public String EventName{ get; set; }
        public Guid AttendeeId { get; set; }
        public String Attendee { get; set; }
        public DateTime TicketDate { get; set; }
        public String Status { get; set; }
        public String Token { get; set; }
    }
}
