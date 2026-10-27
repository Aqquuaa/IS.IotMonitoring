using IS.IotMonitoring.CoreApi.Domain.Entities;
using IS.IotMonitoring.CoreApi.Domain.Enums;

namespace IS.IotMonitoring.CoreApi.Application.DTOs
{
    public record DeviceResponseDto(Guid Id, string Name, DeviceType Type, DeviceStatus Status, DateTimeOffset? LastMessageAt)
    {
        public static DeviceResponseDto From(Device device) =>
            new(device.Id, device.Name, device.Type, device.Status, device.LastMessageAt);
    }
}
