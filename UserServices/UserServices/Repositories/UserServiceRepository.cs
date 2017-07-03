using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserServices.Entities;
using UserServicesDotNetCore.Entities;

namespace UserServices.Repositories {
    public class UserServiceRepository : IUserServiceRepository {

        private AppDbContext _context;

        public UserServiceRepository(AppDbContext context) {
            _context = context;
        }

        public async Task<List<UserEntity>> GetUsers() {
            return await _context.Set<UserEntity>().ToListAsync();
        }

        public async Task<UserEntity> GetUser(Guid id) {
            return await _context.FindAsync<UserEntity>(id);
        }

        public async Task Insert(UserEntity user) {
            user.Id = Guid.NewGuid();
            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task Update(UserEntity user) {
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(UserEntity user) {
            _context.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
