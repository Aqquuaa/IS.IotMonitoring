using IS.IotMonitoring.CoreApi.Application.DTOs;
using IS.IotMonitoring.CoreApi.Application.Metrics;
using IS.IotMonitoring.CoreApi.Application.Repositories;
using IS.IotMonitoring.CoreApi.Domain.Common;

namespace IS.IotMonitoring.CoreApi.Application.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly ITelemetryRepository telemetryRepository;
        private readonly IDeviceRepository deviceRepository;

        private readonly TelemetryMetrics metrics;
        public TelemetryService(ITelemetryRepository telemetryRepository, IDeviceRepository deviceRepository, TelemetryMetrics metrics)
        {
            this.telemetryRepository = telemetryRepository;
            this.deviceRepository = deviceRepository;
            this.metrics = metrics;
        }
        public async Task<Result<IAsyncEnumerable<TelemetryResponseDto>>> GetDeviceTelemetry(Guid deviceId, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            var device = await deviceRepository.GetByIdAsync(deviceId);
            if (device is null)
                return Error.NotFound($"Device '{deviceId}' was not found.");

            var readings = telemetryRepository.GetByDeviceIdAsync(deviceId, from, to);

            return Result.Success(StreamTelemetry(deviceId, from, to));
        }

        public async Task<Result> RecordTelemetry(RecordTelemetryRequest request)
        {
            var device = await deviceRepository.GetByIdAsync(request.DeviceId);
            if (device is null)
                return Error.NotFound($"Device '{request.DeviceId}' was not found.");

            var recordResult = device.RecordTelemetry(request.Reading!.Value, request.Unit, request.Timestamp!.Value);
            if (!recordResult.IsSuccess)
                return recordResult.Error!;

            await telemetryRepository.AddAsync(recordResult.Value!);
            await deviceRepository.UpdateAsync(device);

            metrics.RecordReading(device, request.Reading!.Value);
            return Result.Success();
        }
        private async IAsyncEnumerable<TelemetryResponseDto> StreamTelemetry(Guid deviceId, DateTimeOffset? from, DateTimeOffset? to)
        {
            await foreach (var reading in telemetryRepository.GetByDeviceIdAsync(deviceId, from, to))
            {
                yield return TelemetryResponseDto.From(reading);
            }
        }
    }
}
