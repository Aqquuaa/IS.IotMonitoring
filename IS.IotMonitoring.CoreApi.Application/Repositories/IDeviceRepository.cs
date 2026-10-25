using IS.IotMonitoring.CoreApi.Domain.Entities;

namespace IS.IotMonitoring.CoreApi.Application.Repositories
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByIdAsync(Guid id);
        Task<IReadOnlyCollection<Device>> GetAllAsync();
        Task AddAsync(Device device);
        Task UpdateAsync(Device device);
    }
}
