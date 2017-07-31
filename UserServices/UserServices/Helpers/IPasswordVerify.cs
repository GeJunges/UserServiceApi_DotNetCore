using UserServicesDotNetCore.Entities;

namespace UserServices.Helpers {
    public interface IPasswordVerify {
        bool VerifyHashedPassword(UserEntity user, string providedPassword);
    }
}