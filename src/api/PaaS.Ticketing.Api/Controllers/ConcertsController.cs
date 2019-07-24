using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaaS.Ticketing.ApiLib.DTOs;
using PaaS.Ticketing.ApiLib.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.Controllers
{
    [Route("core/v1/[controller]")]
    [ApiController]
    public class ConcertsController : ControllerBase
    {
        private readonly IConcertsRepository _concertsRepository;

        public ConcertsController(IConcertsRepository concertsRepository)
        {
            _concertsRepository = concertsRepository;
        }


        /// <summary>
        /// Get concerts
        /// </summary>
        /// <remarks>Provides a complete object for all known concerts</remarks>
        /// <returns>Return a list of Concerts</returns>
        [HttpGet(Name = "Concerts_GetAllConcerts")]
        [SwaggerResponse((int)HttpStatusCode.OK, "List of concerts", typeof(IEnumerable<ConcertDto>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/json","application/problem+json")]
        public async Task<IActionResult> GetAllConcerts()
        {
            var concerts = await _concertsRepository.GetConcertsAsync();
            var results = Mapper.Map<IEnumerable<ConcertDto>>(concerts);
            return Ok(concerts);
        }

        /// <summary>
        /// Get single concert
        /// </summary>
        /// <param name="id">Concert identifier</param>
        /// <remarks>Get information of a single concert</remarks>
        /// <returns>Return a single concert</returns>
        [HttpGet("{id}", Name = "Concerts_GetConcert")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Concert data object", typeof(ConcertDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Concert not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> GetConcert(Guid id)
        {
            var concert = await _concertsRepository.GetConcertAsync(id);
            if (concert == null)
            {
                return NotFound();
            }
            var result = Mapper.Map<ConcertDto>(concert);
            return Ok(result);
        }


        /// <summary>
        /// Get users of a concert
        /// </summary>
        /// <param name="id">Concert identifier</param>
        /// <remarks>Get users of a single concert</remarks>
        /// <returns>Return a list of users</returns>
        [HttpGet("{id}/users", Name = "Concerts_GetUsersOfConcert")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Users list", typeof(UserDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Concert not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> GetUsersByConcert(Guid id)
        {
            var concert = await _concertsRepository.GetConcertAsync(id);
            if (concert == null)
            {
                return NotFound();
            }

            var users = await _concertsRepository.GetUsersOfConcertAsync(concert.ConcertId);
            var results = Mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(results);
        }
    }
}