using System;

namespace PaaS.Ticketing.ApiLib.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
    }
}
