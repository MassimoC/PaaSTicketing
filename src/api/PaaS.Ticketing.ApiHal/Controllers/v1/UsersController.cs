using AutoMapper;
using Halcyon.HAL;
using Microsoft.AspNetCore.Mvc;
using PaaS.Ticketing.ApiLib.DTOs;
using PaaS.Ticketing.ApiLib.Entities;
using PaaS.Ticketing.ApiLib.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiHal.Controllers.v1
{
    [Route("core/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepository _usersRepository;

        public UsersController(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        /// <summary>
        /// Get users
        /// </summary>
        /// <remarks>Provides a complete object for all known users</remarks>
        /// <returns>Return a list of Concerts</returns>
        [HttpGet(Name = "Users_GetAllUsers")]
        [SwaggerResponse((int)HttpStatusCode.OK, "List of users", typeof(IEnumerable<UserDto>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/hal+json", "application/problem+json")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _usersRepository.GetUsersAsync();
            var results = Mapper.Map<IEnumerable<UserDto>>(users);

            var response = new HALResponse(null)
                            .AddLinks(new Link("self", $"/core/v1/users"))
                            .AddEmbeddedCollection("user", results,
                                new Link[] {
                                    new Link("self", "/core/v1/users/{UserId}")
                                });
            return Ok(response);

        }

        /// <summary>
        /// Get single user
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <remarks>Get information of a single user</remarks>
        /// <returns>Return a single user</returns>
        [HttpGet("{id}", Name = "Users_GetUserProfile")]
        [SwaggerResponse((int)HttpStatusCode.OK, "User profile object", typeof(UserDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "User not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/hal+json", "application/problem+json")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
            var user = await _usersRepository.GetUserAsync(id);
            var results = Mapper.Map<UserDto>(user);

            var response = new HALResponse(results)
                            .AddLinks(new Link("self", $"/core/v1/users/{id}"))
                            .AddLinks(new Link("profiles", "/core/v1/profiles/{Email}"));
            return Ok(response);
        }


        /// <summary>
        /// Create a new User
        /// </summary>
        /// <param name="user">User json representation</param>
        /// <remarks>Create a new userr</remarks>
        /// <returns>Return the new user</returns>
        [HttpPost(Name = "Users_CreateUserProfile")]
        [SwaggerResponse((int)HttpStatusCode.Created, "User profile created", typeof(UserCreateDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "User not found", typeof(ProblemDetails))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Consumes("application/json")]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> CreateUserProfile([FromBody] UserCreateDto user)
        {
            var entityUser = Mapper.Map<User>(user);
            _usersRepository.AddUser(entityUser);
            await _usersRepository.SaveChangesAsync();

            await _usersRepository.GetUserAsync(entityUser.UserId);

            return CreatedAtRoute("Users_GetUserProfile",
                new { id = entityUser.UserId},
                entityUser);
        }

    }
}