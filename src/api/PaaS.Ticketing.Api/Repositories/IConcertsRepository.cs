using PaaS.Ticketing.Api.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.Repositories
{
    public interface IConcertsRepository
    {
        Task<IEnumerable<Concert>> GetConcertsAsync();
        Task<Concert> GetConcertAsync(Guid id);
        Task<IEnumerable<User>> GetUsersOfConcertAsync(Guid concertId);
    }
}
