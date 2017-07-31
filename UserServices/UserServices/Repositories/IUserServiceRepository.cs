using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserServicesDotNetCore.Entities;

namespace UserServices.Repositories {
    public interface IUserServiceRepository {
        Task<List<UserEntity>> GetUsers();
        Task<UserEntity> GetUserById(Guid id);
        Task<UserEntity> GetUserByEmail(string email);
        Task Insert(UserEntity user);
        Task Update(UserEntity user);
        Task Delete(UserEntity user);
    }
}