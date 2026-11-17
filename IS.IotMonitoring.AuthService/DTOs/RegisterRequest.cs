using System.ComponentModel.DataAnnotations;

namespace IS.IotMonitoring.AuthService.DTOs
{
    // BCrypt only uses the first 72 bytes of a password, hence the upper limit.
    public record RegisterRequest(
        [StringLength(100, MinimumLength = 3)] string Username,
        [StringLength(72, MinimumLength = 8)] string Password);
}
