using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaaS.Ticketing.Api.Entities
{
    [Table("ConcertUsers")]
    public class ConcertUser
    {
        [Key]
        public Guid ConcertUserId { get; set; }
        [Required]
        public Guid ConcertId { get; set; }
        public Concert Concert { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public String Status { get; set; }
        public DateTime DateRegistration { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Token { get; set; }
    }
}
