using PaaS.Ticketing.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.Repositories
{
    public interface IOrdersRepository
    {
        void PlaceOrderAsync(ConcertUser order);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, String status);
        Task<bool> UpdateOrderAsync(ConcertUser order);
        Task<bool> SaveChangesAsync();
        Task<ConcertUser> GetOrderAsync(String token);
        Task<ConcertUser> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<ConcertUser>> GetOrdersAsync(string status);
    }
}
