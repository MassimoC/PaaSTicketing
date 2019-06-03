using PaaS.Ticketing.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.Repositories
{
    public interface IUsersRepository
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> GetUserAsync(Guid id);
        void AddUser(User user);
        Task<bool> SaveChangesAsync();
    }
}
