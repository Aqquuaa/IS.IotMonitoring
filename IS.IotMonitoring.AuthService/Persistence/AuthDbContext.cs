using IS.IotMonitoring.AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace IS.IotMonitoring.AuthService.Persistence
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("users");
                builder.HasKey(u => u.Id);

                builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
                builder.HasIndex(u => u.Username).IsUnique();

                builder.Property(u => u.PasswordHash).IsRequired();

                builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
            });
        }
    }
}
