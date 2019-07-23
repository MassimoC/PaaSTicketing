using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaaS.Ticketing.ApiLib.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Firstname { get; set; }
        [Required]
        [MaxLength(50)]
        public string Lastname { get; set; }
        [Required]
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<ConcertUser> ConcertUser { get; set; }
    }
}
