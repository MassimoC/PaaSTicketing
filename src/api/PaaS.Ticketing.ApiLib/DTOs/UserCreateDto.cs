using System.ComponentModel.DataAnnotations;

namespace PaaS.Ticketing.ApiLib.DTOs
{
    public class UserCreateDto
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [Required]
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
