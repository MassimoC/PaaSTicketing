using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib.DTOs
{

    public class OrderCreateDto
    {
        public Guid UserId { get; set; }
        public Guid ConcertId { get; set; }
        public DateTime TicketDate { get; set; }
    }
}
