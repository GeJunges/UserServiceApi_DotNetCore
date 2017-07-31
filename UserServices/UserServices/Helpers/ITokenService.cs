using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using UserServicesDotNetCore.Entities;

namespace UserServices.Helpers {
    public interface ITokenService {
        JwtSecurityToken GetJwtSecurityToken(UserEntity user);
    }
}