using Microsoft.EntityFrameworkCore;
using UserServicesDotNetCore.Entities;

namespace UserServices.Entities {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
            Database.Migrate();
        }

        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>(entity => {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.CpfCnpj).IsUnique();
            });
        }
    }
}