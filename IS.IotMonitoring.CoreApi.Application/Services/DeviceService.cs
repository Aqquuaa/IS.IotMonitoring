using IS.IotMonitoring.CoreApi.Application.DTOs;
using IS.IotMonitoring.CoreApi.Application.Repositories;
using IS.IotMonitoring.CoreApi.Domain.Common;
using IS.IotMonitoring.CoreApi.Domain.Entities;

namespace IS.IotMonitoring.CoreApi.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository repository;
        public DeviceService(IDeviceRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Result<DeviceResponseDto>> CreateDeviceAsync(CreateDeviceRequest request)
        {
            var device = Device.Create(request.Name, request.Type);

            await repository.AddAsync(device);

            return DeviceResponseDto.From(device);
        }
        public async Task<IReadOnlyCollection<DeviceResponseDto>> GetAllAsync()
        {
            var devices = await repository.GetAllAsync();
            return devices.Select(d => DeviceResponseDto.From(d)).ToList();
        }
        public async Task<Result<DeviceResponseDto>> GetByIdAsync(Guid id)
        {
            var device = await repository.GetByIdAsync(id);
            if (device is null)
                return Error.NotFound($"Device with ID '{id}' was not found.");

            return DeviceResponseDto.From(device);
        }
        public async Task<Result> DeleteDeviceAsync(Guid id)
        {
            var device = await repository.GetByIdAsync(id);

            if (device is null)
                return Error.NotFound($"Device with ID '{id}' was not found.");

            device.Delete();
            await repository.UpdateAsync(device);

            return Result.Success();
        }
    }
}
