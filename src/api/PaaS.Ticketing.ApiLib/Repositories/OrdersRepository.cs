using PaaS.Ticketing.ApiLib.Context;
using PaaS.Ticketing.ApiLib.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaaS.Ticketing.ApiLib.Extensions;

namespace PaaS.Ticketing.ApiLib.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {

        private readonly TicketingContext _context;

        public OrdersRepository(TicketingContext context)
        {
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public void PlaceOrderAsync(ConcertUser order)
        {
            _context.Add(order);
        }

        
        public async Task<ConcertUser> GetOrderAsync(String token)
        {
            return await _context.Orders
                .Include(c => c.Concert)
                .Include(u => u.User)
                .Where(p => p.Token == token).FirstOrDefaultAsync();
        }

        public async Task<ConcertUser> GetOrderByIdAsync(Guid id)
        {
            return await _context.Orders.Where(p => p.ConcertUserId == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ConcertUser>> GetOrdersAsync(string status)
        {
            if (! String.IsNullOrEmpty(status))
            {
                return await _context.Orders.Where(p => p.Status == status).ToListAsync();
            }
            return await  _context.Orders
                .Include(c => c.Concert)
                .Include(u => u.User).ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
        {
            var order = await _context.Orders.Where(p => p.ConcertUserId== orderId).FirstOrDefaultAsync();
            order.Status = status;
            _context.Add(order);
            return (await _context.SaveChangesAsync() > 0);
        }

        public async Task<bool> UpdateOrderAsync(ConcertUser order)
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            _context.Orders.Update(order);
            return (await _context.SaveChangesAsync() > 0);

        }
    }
}
