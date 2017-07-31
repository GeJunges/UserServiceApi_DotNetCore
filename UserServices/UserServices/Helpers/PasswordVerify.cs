using UserServicesDotNetCore.Entities;

namespace UserServices.Helpers {
    public class PasswordVerify : IPasswordVerify {
        public bool VerifyHashedPassword(UserEntity user, string providedPassword) {
            
            if (BCrypt.Net.BCrypt.Verify(providedPassword, user.Password)) {
                return true;
            }

            return false;
        }
    }
}
