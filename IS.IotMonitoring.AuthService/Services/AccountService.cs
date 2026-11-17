using IS.IotMonitoring.AuthService.DTOs;
using IS.IotMonitoring.AuthService.Entities;
using IS.IotMonitoring.AuthService.Persistence;
using IS.IotMonitoring.AuthService.Security;
using Microsoft.EntityFrameworkCore;

namespace IS.IotMonitoring.AuthService.Services
{
    public class AccountService : IAccountService
    {
        private readonly AuthDbContext context;
        private readonly IJwtTokenGenerator tokenGenerator;

        public AccountService(AuthDbContext context, IJwtTokenGenerator tokenGenerator)
        {
            this.context = context;
            this.tokenGenerator = tokenGenerator;
        }

        public async Task<UserResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            var username = User.NormalizeUsername(request.Username);
            if (await context.Users.AnyAsync(u => u.Username == username, cancellationToken))
                return null;

            var user = User.Create(request.Username, request.Password, UserRole.User);
            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            return UserResponse.From(user);
        }

        public async Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var username = User.NormalizeUsername(request.Username);
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

            if (user is null || !user.VerifyPassword(request.Password))
                return null;

            return tokenGenerator.Generate(user);
        }
    }
}
