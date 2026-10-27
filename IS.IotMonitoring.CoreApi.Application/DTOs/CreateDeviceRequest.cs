using IS.IotMonitoring.CoreApi.Domain.Enums;

namespace IS.IotMonitoring.CoreApi.Application.DTOs
{
    public record CreateDeviceRequest(string Name, DeviceType Type);
}
