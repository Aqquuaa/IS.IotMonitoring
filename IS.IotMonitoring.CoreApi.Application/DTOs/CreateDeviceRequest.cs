using System.ComponentModel.DataAnnotations;
using IS.IotMonitoring.CoreApi.Domain.Enums;

namespace IS.IotMonitoring.CoreApi.Application.DTOs
{
    public record CreateDeviceRequest(
        string Name,
        [EnumDataType(typeof(DeviceType))] DeviceType Type);
}
