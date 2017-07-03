using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserServicesDotNetCore.Entities;

namespace UserServices.Repositories {
    public interface IUserServiceRepository {
        Task<List<UserEntity>> GetUsers();
        Task<UserEntity> GetUser(Guid id);
        Task Insert(UserEntity user);
        Task Update(UserEntity user);
        Task Delete(UserEntity user);
    }
}