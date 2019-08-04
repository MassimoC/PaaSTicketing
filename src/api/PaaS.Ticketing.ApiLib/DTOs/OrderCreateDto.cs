using System;
using System.ComponentModel.DataAnnotations;

namespace PaaS.Ticketing.ApiLib.DTOs
{

    public class OrderCreateDto
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid ConcertId { get; set; }
        public DateTime TicketDate { get; set; }
    }
}
