using PaaS.Ticketing.ApiLib.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib.Repositories
{
    public interface IConcertsRepository
    {
        Task<IEnumerable<Concert>> GetConcertsAsync();
        Task<Concert> GetConcertAsync(Guid id);
        Task<Concert> GetConcertExpandedAsync(Guid id);
        Task<IEnumerable<User>> GetUsersOfConcertAsync(Guid concertId);
    }
}
