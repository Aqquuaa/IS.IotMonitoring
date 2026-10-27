namespace IS.IotMonitoring.CoreApi.Application.DTOs
{
    public record RecordTelemetryRequest(Guid DeviceId, double Reading, string Unit, DateTimeOffset Timestamp);
}
