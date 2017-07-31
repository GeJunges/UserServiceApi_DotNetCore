using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using UserServicesDotNetCore.Entities;
using System.Security.Claims;
using System;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace UserServices.Helpers {
    public class TokenService : ITokenService {
        private IConfigurationRoot _configRoot;

        public TokenService(IConfigurationRoot config) {
            _configRoot = config;
        }

        public JwtSecurityToken UpdateJwtSecurityToken(JwtSecurityToken token, UserEntity user) {

            return new JwtSecurityToken();
        }

        public UserEntity DecodeJwtSecurityToken(JwtSecurityToken token) {

            return null;
        }

        public JwtSecurityToken GetJwtSecurityToken(UserEntity user) {
            var claims = CreateClaims(user);
            var creds = CreateCredentials();
            var expires = int.Parse(_configRoot["JWTSecurity:Expiration"]);

            return new JwtSecurityToken(
                issuer: _configRoot["JWTSecurity:Issuer"],
                audience: _configRoot["JWTSecurity:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(expires),
                signingCredentials: creds
                );
        }

        private SigningCredentials CreateCredentials() {
            var secretKey = _configRoot["JWTSecurity:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        private Claim[] CreateClaims(UserEntity user) {
            return new Claim[] {
                new Claim( JwtRegisteredClaimNames.Sub, user.Email),
                new Claim( JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            };
        }
    }
}
