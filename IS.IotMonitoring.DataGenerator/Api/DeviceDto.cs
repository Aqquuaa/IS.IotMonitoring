namespace IS.IotMonitoring.DataGenerator.Api
{
    public record DeviceDto(Guid Id, string Name, DeviceType Type, DeviceStatus Status);
}
