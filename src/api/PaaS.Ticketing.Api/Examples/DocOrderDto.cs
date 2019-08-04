using PaaS.Ticketing.ApiLib.DTOs;
using Swashbuckle.AspNetCore.Filters;
using System;

namespace PaaS.Ticketing.Api.Examples
{
    public class DocOrderDto : IExamplesProvider
    {
        public object GetExamples()
        {
            return new OrderDto
            {
                OrderId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventName = "Best Kept Secret Festival 2019",
                AttendeeId = Guid.NewGuid(),
                Attendee = "Michelle Obama",
                TicketDate = DateTime.UtcNow,
                Status = "Pending",
                Token = "2--038"
            };

        }
    }
}
