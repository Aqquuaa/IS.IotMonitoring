using IS.IotMonitoring.AuthService.Entities;
using IS.IotMonitoring.AuthService.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IS.IotMonitoring.AuthService.Persistence
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            await context.Database.EnsureCreatedAsync();

            var seed = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
            if (string.IsNullOrWhiteSpace(seed.Username) || string.IsNullOrWhiteSpace(seed.Password))
                return;

            var username = User.NormalizeUsername(seed.Username);
            if (await context.Users.AnyAsync(u => u.Username == username))
                return;

            context.Users.Add(User.Create(seed.Username, seed.Password, UserRole.Admin));
            await context.SaveChangesAsync();
        }
    }
}
