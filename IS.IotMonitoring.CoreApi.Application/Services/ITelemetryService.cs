using IS.IotMonitoring.CoreApi.Application.DTOs;
using IS.IotMonitoring.CoreApi.Domain.Common;

namespace IS.IotMonitoring.CoreApi.Application.Services
{
    public interface ITelemetryService
    {
        Task<Result> RecordTelemetry(RecordTelemetryRequest request);
        Task<Result<IReadOnlyCollection<TelemetryResponseDto>>> GetDeviceTelemetry(Guid deviceId, DateTimeOffset? from = null, DateTimeOffset? to = null);
    }
}
