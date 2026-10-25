using IS.IotMonitoring.CoreApi.Domain.Entities;

namespace IS.IotMonitoring.CoreApi.Application.Repositories
{
    public interface ITelemetryRepository
    {
        Task<IReadOnlyCollection<TelemetryReading>> GetByDeviceIdAsync(Guid deviceId, DateTimeOffset? from, DateTimeOffset? to);
        Task AddAsync(TelemetryReading reading);
    }
}
