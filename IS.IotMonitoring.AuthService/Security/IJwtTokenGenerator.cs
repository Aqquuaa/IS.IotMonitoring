using IS.IotMonitoring.AuthService.DTOs;
using IS.IotMonitoring.AuthService.Entities;

namespace IS.IotMonitoring.AuthService.Security
{
    public interface IJwtTokenGenerator
    {
        TokenResponse Generate(User user);
    }
}
