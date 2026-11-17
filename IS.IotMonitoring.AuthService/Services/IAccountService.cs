using IS.IotMonitoring.AuthService.DTOs;

namespace IS.IotMonitoring.AuthService.Services
{
    public interface IAccountService
    {
        // Returns null when the username is already taken.
        Task<UserResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

        // Returns null when the username or password is wrong.
        Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    }
}
