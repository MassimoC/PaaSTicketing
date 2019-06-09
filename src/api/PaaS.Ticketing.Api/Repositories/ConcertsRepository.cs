﻿using PaaS.Ticketing.Api.Context;
using PaaS.Ticketing.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PaaS.Ticketing.Api.Repositories
{
    public class ConcertsRepository : IConcertsRepository
    {
        private readonly TicketingContext _context;

        public ConcertsRepository(TicketingContext context)
        {
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<Concert> GetConcertAsync(Guid concertId)
        {
            return await _context.Concerts.Where(p => p.ConcertId == concertId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Concert>> GetConcertsAsync()
        {
            return await _context.Concerts.ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersOfConcertAsync(Guid concertId)
        {
            return await _context.Users.Where(p => p.ConcertUser.Any(x => x.ConcertId == concertId)).ToListAsync();
        }
    }
}