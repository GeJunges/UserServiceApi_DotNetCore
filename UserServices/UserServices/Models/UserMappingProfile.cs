using AutoMapper;
using UserServicesDotNetCore.Entities;

namespace UserServices.Models {
    public class UserMappingProfile : Profile {

        public UserMappingProfile() {
            CreateMap<UserEntity, User>();
            CreateMap<User, UserEntity>();
        }
    }
}
