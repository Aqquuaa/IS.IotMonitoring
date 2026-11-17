using IS.IotMonitoring.AuthService.Entities;

namespace IS.IotMonitoring.AuthService.DTOs
{
    public record UserResponse(Guid Id, string Username, string Role, DateTimeOffset CreatedAt)
    {
        public static UserResponse From(User user) =>
            new(user.Id, user.Username, user.Role.ToString(), user.CreatedAt);
    }
}
