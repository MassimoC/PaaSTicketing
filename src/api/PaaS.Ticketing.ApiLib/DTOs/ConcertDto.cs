using System;
using System.Collections.Generic;

namespace PaaS.Ticketing.ApiLib.DTOs
{
    public class ConcertDto
    {
        public Guid ConcertId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Location { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<TicketDto> Tickets { get; set; }
    }

    public class TicketDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
