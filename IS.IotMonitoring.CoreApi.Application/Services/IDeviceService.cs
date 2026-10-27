using IS.IotMonitoring.CoreApi.Application.DTOs;
using IS.IotMonitoring.CoreApi.Domain.Common;

namespace IS.IotMonitoring.CoreApi.Application.Services
{
    public interface IDeviceService
    {
        Task<Result<DeviceResponseDto>> CreateDeviceAsync(CreateDeviceRequest request);
        Task<Result<DeviceResponseDto>> GetByIdAsync(Guid id);
        Task<IReadOnlyCollection<DeviceResponseDto>> GetAllAsync();
        Task<Result> DeleteDeviceAsync(Guid id);
    }
}
