using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Threading.Tasks;
using UserServices.Helpers;
using UserServices.Models;
using UserServices.Repositories;

namespace UserServices.Controllers {
    [Route("api/login")]
    public class LoginController : BaseController {

        private static IUserServiceRepository _userRepository;
        private ITokenService _tokenService;
        private IPasswordVerify _passwordVerify;

        public LoginController(IUserServiceRepository userRepository, IPasswordVerify passwordVerify,
            ITokenService tokenService) {
            _userRepository = userRepository;
            _passwordVerify = passwordVerify;
            _tokenService = tokenService;
        }

        [HttpPost("Authenticate")]
        [ValidateModel]
        public async Task<IActionResult> Authenticate([FromBody]CredentialModel model) {
            try {
                var user = await _userRepository.GetUserByEmail(model.Email);

                if (user == null) {
                    return StatusCode((int)HttpStatusCode.NotFound, "Email does not exist");
                }

                if (!_passwordVerify.VerifyHashedPassword(user, model.Password)) {
                    return StatusCode((int)HttpStatusCode.NotFound, "Password does not exist");
                }

                var token = _tokenService.GetJwtSecurityToken(user);

                return Ok(new {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            catch (Exception ex) {

                return BadRequest($"Failed to login: {ex.Message}");
            }
        }
    }
}
