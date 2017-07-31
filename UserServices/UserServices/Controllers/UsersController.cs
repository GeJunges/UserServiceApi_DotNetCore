using Microsoft.AspNetCore.Mvc;
using UserServices.Repositories;
using System;
using System.Net;
using UserServicesDotNetCore.Entities;
using AutoMapper;
using System.Collections.Generic;
using UserServices.Models;
using UserServices.Helpers;
using System.Threading.Tasks;
using UserServices.Middlewares;
using Microsoft.AspNetCore.Authorization;

namespace UserServices.Controllers {
    [Authorize]
    [Route("api/users")]
    public class UsersController : Controller {

        private IUserServiceRepository _userServiceRepository;
        private IMapper _autoMapper;

        public UsersController(IMapper autoMapper, IUserServiceRepository userServiceRepository) {
            _userServiceRepository = userServiceRepository;
            _autoMapper = autoMapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                var usersEntity = await _userServiceRepository.GetUsers();

                var users = _autoMapper.Map<List<User>>(usersEntity);
                return Ok(users);
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id) {
            try {
                var userEntity = await _userServiceRepository.GetUserById(id);
                if (userEntity == null) {
                    return StatusCode((int)HttpStatusCode.NotFound, "user not foud");
                }
                var user = _autoMapper.Map<User>(userEntity);

                return Ok(user);
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [MiddlewareFilter(typeof(PasswordEncryptor))]
        public async Task<IActionResult> Register([FromBody]User user) {
            try {
                if (!ModelState.IsValid) {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var exist = _userServiceRepository.GetUserByEmail(user.Email).Result;
                if (exist != null) {
                    return StatusCode((int)HttpStatusCode.BadRequest, "E-mail already registered!");
                }

                var userEntity = _autoMapper.Map<UserEntity>(user);
                await _userServiceRepository.Insert(userEntity);
                return StatusCode((int)HttpStatusCode.Created, "Success!");
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody]User user) {
            try {
                if (!ModelState.IsValid) {
                    return new UnprocessableEntityObjectResult(ModelState);
                }
                var userEntity = await _userServiceRepository.GetUserById(id);

                if (userEntity == null) {
                    return StatusCode((int)HttpStatusCode.NotFound, "user not foud");
                }

                var userToUpdate = _autoMapper.Map(user, userEntity);

                await _userServiceRepository.Update(userToUpdate);
                return StatusCode((int)HttpStatusCode.OK, "Success!");
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id) {
            try {
                var userEntity = await _userServiceRepository.GetUserById(id);

                if (userEntity == null) {
                    return StatusCode((int)HttpStatusCode.NotFound, "user not foud");
                }

                await _userServiceRepository.Delete(userEntity);

                return StatusCode((int)HttpStatusCode.OK, "Success!");
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
