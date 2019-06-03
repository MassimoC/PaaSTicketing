using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaaS.Ticketing.Api.DTOs;
using PaaS.Ticketing.Api.Entities;
using PaaS.Ticketing.Api.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.Controllers
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
        /// Get concerts
        /// </summary>
        /// <remarks>Provides a complete object for all known concerts</remarks>
        /// <returns>Return a list of Concerts</returns>
        [HttpGet(Name = "Users_GetAllUsers")]
        [SwaggerResponse((int)HttpStatusCode.OK, "List of users")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _usersRepository.GetUsersAsync();
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
        [SwaggerResponse((int)HttpStatusCode.OK, "User profile object")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "User not found")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available")]
        [Produces("application/json")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
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
        [SwaggerResponse((int)HttpStatusCode.Created, "User profile created")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "User not found")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available")]
        [Consumes("application/json")]
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