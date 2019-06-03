using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.DTOs
{
    public class ConcertDto
    {
        public Guid ConcertId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Location { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
