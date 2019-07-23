using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PaaS.Ticketing.ApiLib.Entities
{
    [Table("Concerts")]
    public class Concert
    {
        [Key]
        public Guid ConcertId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public DateTime From { get; set; }
        [Required]
        public DateTime To { get; set; }
        public ICollection<ConcertUser> ConcertUser { get; set; }
    }
}
