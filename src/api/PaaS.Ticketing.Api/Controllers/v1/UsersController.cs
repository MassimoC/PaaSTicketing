using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaaS.Ticketing.ApiLib.DTOs;
using PaaS.Ticketing.ApiLib.Entities;
using PaaS.Ticketing.ApiLib.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.Controllers.v1
{
    [Route("core/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger _logger;

        public UsersController(IUsersRepository usersRepository, ILogger<UsersController> logger)
        {
            _usersRepository = usersRepository;
            _logger = logger;
        }


        /// <summary>
        /// Get users
        /// </summary>
        /// <param name="page"></param>
        /// <remarks>Provides a complete object for all known users</remarks>
        /// <returns>Return a list of Concerts</returns>
        [HttpGet(Name = "Users_GetAllUsers")]
        [SwaggerResponse((int)HttpStatusCode.OK, "List of users", typeof(IEnumerable<UserDto>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available", typeof(ProblemDetails))]
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> GetAllUsers(int? page = null)
        {
            _logger.LogInformation($"GetAllUser - page[{page}]");
            var users = await _usersRepository.GetUsersAsync(page);
            _logger.LogDebug("GetAllUser - map to DTOs");
            var result = Mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(result);
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
        [Produces("application/json", "application/problem+json")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
            if (id.ToString() == "00000000-0000-0000-0000-000000000000")
                    throw new ArgumentException("Value not supported");
            var user = await _usersRepository.GetUserAsync(id);
            var result = Mapper.Map<UserDto>(user);
            return Ok(result);
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
        //[Produces("application/json", "application/problem+json")]
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