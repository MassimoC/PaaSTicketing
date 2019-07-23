using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
    }
}
