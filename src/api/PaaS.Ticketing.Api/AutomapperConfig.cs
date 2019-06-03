using Microsoft.AspNetCore.JsonPatch;
using PaaS.Ticketing.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api
{
    public static class AutoMapperConfig
    {
        private static object _thisLock = new object();
        private static bool _initialized = false;

        public static void Initialize()
        {
            lock (_thisLock)
            {
                if (!_initialized)
                {
                    AutoMapper.Mapper.Initialize(mapperConfig =>
                    {
                        // inbound
                        mapperConfig.CreateMap<DTOs.OrderCreateDto, Entities.ConcertUser>()
                            .ForMember(dest => dest.DateRegistration, opt => opt.MapFrom(src => src.TicketDate))
                            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => Guid.NewGuid().ToString().RandomString()))
                            .ForMember(dest => dest.ConcertUserId, opt => opt.MapFrom(src => Guid.NewGuid()));
                        mapperConfig.CreateMap<DTOs.UserCreateDto, Entities.User>()
                            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.NewGuid()));

                        // outbound
                        mapperConfig.CreateMap<Entities.Concert, DTOs.ConcertDto>();
                        mapperConfig.CreateMap<Entities.User, DTOs.UserDto>();
                        mapperConfig.CreateMap<Entities.ConcertUser, DTOs.OrderDto>()
                            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ConcertUserId))
                            .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.Concert.Name))
                            .ForMember(dest => dest.Attendee, opt => opt.MapFrom(src => String.Format("{0} {1}", src.User.Firstname, src.User.Lastname)));
                    });
                    _initialized = true;
                }
            }
        }
    }
}
