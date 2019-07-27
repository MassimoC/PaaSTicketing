using PaaS.Ticketing.ApiLib.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib.Repositories
{
    public interface IUsersRepository
    {
        Task<IEnumerable<User>> GetUsersAsync(int? page);
        Task<User> GetUserAsync(Guid id);
        void AddUser(User user);
        Task<bool> SaveChangesAsync();
    }
}
