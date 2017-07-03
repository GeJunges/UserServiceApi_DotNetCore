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

namespace UserServices.Controllers {
    [Route("api/user")]
    public class UserController : Controller {

        private IUserServiceRepository _userServiceRepository;

        public UserController(IUserServiceRepository userServiceRepository) {
            _userServiceRepository = userServiceRepository;
        }

        [HttpGet]
        [Route("GetUsers")]
        public IActionResult GetUsers() {
            try {
                var usersEntity = _userServiceRepository.GetUsers();
                var users = Mapper.Map<List<User>>(usersEntity.Result);
                return Ok(users);
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetUser/{id}")]
        public async Task<IActionResult> GetUser(Guid id) {
            try {
                var userEntity = await _userServiceRepository.GetUser(id);
                if (userEntity == null) {
                    return StatusCode((int)HttpStatusCode.NotFound, "user not foud");
                }
                var user = Mapper.Map<User>(userEntity);

                return Ok(user);
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]User user) {
            try {
                if (!ModelState.IsValid) {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var userEntity = Mapper.Map<UserEntity>(user);
                await _userServiceRepository.Insert(userEntity);
                return StatusCode((int)HttpStatusCode.Created, "Success!");
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [Route("Updade")]
        public async Task<IActionResult> Updade([FromBody]User user) {
            try {
                if (!ModelState.IsValid) {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var userEntity = Mapper.Map<UserEntity>(user);
                await _userServiceRepository.Update(userEntity);
                return StatusCode((int)HttpStatusCode.OK, "Success!");
            }
            catch (Exception ex) {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id) {
            try {
                var userEntity = await _userServiceRepository.GetUser(id);

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
