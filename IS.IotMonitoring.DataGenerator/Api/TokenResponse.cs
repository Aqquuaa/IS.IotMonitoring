namespace IS.IotMonitoring.DataGenerator.Api
{
    public record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
}
