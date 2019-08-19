using PaaS.Ticketing.ApiLib.DTOs;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace PaaS.Ticketing.Api.Examples
{
    public class DocUserDto : IExamplesProvider
    {
        public object GetExamples()
        {
            return new UserDto
            {
                UserId = Guid.NewGuid(),
                Email = "first.second@email.com"
            };
        }
    }

    public class DocUserDtoList : IExamplesProvider
    {
        public object GetExamples()
        {
            return new List<UserDto> {
                new UserDto {
                    UserId = Guid.NewGuid(),
                    Email = "first.second@email.com"
                },
                new UserDto {
                    UserId = Guid.NewGuid(),
                    Email = "third.forth@email.com"
                }
            };
        }
    }
}
