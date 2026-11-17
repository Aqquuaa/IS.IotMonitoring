using System.ComponentModel.DataAnnotations;

namespace IS.IotMonitoring.CoreApi.Application.DTOs
{
    public record RecordTelemetryRequest(
        Guid DeviceId,
        [Required] double? Reading,
        string Unit,
        [Required] DateTimeOffset? Timestamp);
}
