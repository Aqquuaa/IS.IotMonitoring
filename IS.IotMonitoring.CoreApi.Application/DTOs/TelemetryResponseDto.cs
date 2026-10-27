using IS.IotMonitoring.CoreApi.Domain.Entities;

namespace IS.IotMonitoring.CoreApi.Application.DTOs
{
    public record TelemetryResponseDto(Guid Id, double Reading, string Unit, DateTimeOffset Timestamp)
    {
        public static TelemetryResponseDto From(TelemetryReading telemetry) =>
            new(telemetry.DeviceId, telemetry.Reading, telemetry.Unit, telemetry.Timestamp);
    }
}
