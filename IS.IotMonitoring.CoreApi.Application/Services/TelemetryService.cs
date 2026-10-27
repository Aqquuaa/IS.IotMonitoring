using IS.IotMonitoring.CoreApi.Application.DTOs;
using IS.IotMonitoring.CoreApi.Application.Repositories;
using IS.IotMonitoring.CoreApi.Domain.Common;

namespace IS.IotMonitoring.CoreApi.Application.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly ITelemetryRepository telemetryRepository;
        private readonly IDeviceRepository deviceRepository;
        public TelemetryService(ITelemetryRepository telemetryRepository, IDeviceRepository deviceRepository)
        {
            this.telemetryRepository = telemetryRepository;
            this.deviceRepository = deviceRepository;
        }
        public async Task<Result<IReadOnlyCollection<TelemetryResponseDto>>> GetDeviceTelemetry(Guid deviceId, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            var device = await deviceRepository.GetByIdAsync(deviceId);
            if (device is null)
                return Error.NotFound($"Device '{deviceId}' was not found.");

            var readings = await telemetryRepository.GetByDeviceIdAsync(deviceId, from, to);

            return readings.Select(t => TelemetryResponseDto.From(t)).ToList();
        }

        public async Task<Result> RecordTelemetry(RecordTelemetryRequest request)
        {
            var device = await deviceRepository.GetByIdAsync(request.DeviceId);
            if (device is null)
                return Error.NotFound($"Device '{request.DeviceId}' was not found.");

            var reading = device.RecordTelemetry(request.Reading, request.Unit, request.Timestamp);

            await telemetryRepository.AddAsync(reading);
            await deviceRepository.UpdateAsync(device);

            return Result.Success();
        }
    }
}
