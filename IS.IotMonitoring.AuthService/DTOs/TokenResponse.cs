namespace IS.IotMonitoring.AuthService.DTOs
{
    public record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
}
