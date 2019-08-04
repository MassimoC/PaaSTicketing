using PaaS.Ticketing.ApiLib.DTOs;
using Swashbuckle.AspNetCore.Filters;
using System;

namespace PaaS.Ticketing.Api.Examples
{
    public class DocOrderCreateDto : IExamplesProvider
    {
        public object GetExamples()
        {
            return new OrderCreateDto
            {
                ConcertId = new Guid("97934d7d-7bd2-42fc-977e-4709a7cf08a4"),
                UserId = Guid.NewGuid(),
                TicketDate = DateTime.UtcNow
            };

        }
    }
}
