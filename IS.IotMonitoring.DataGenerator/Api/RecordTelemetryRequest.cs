namespace IS.IotMonitoring.DataGenerator.Api
{
    public record RecordTelemetryRequest(Guid DeviceId, double Reading, string Unit, DateTimeOffset Timestamp);
}
