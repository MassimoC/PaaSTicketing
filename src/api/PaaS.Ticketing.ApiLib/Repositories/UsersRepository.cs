using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaaS.Ticketing.ApiLib.Context;
using PaaS.Ticketing.ApiLib.Entities;

namespace PaaS.Ticketing.ApiLib.Repositories
{
    public class UsersRepository : IUsersRepository
    {

        private readonly TicketingContext _context;

        public UsersRepository(TicketingContext context)
        {
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
        public async Task<User> GetUserAsync(Guid userId)
        {
            return await _context.Users.Where(p => p.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public void AddUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _context.Add(user);
        }
        public async Task<bool> SaveChangesAsync()
        {
            // return true if 1 or more entities were changed
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}
