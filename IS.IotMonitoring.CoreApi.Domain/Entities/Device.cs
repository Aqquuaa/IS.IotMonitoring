using IS.IotMonitoring.CoreApi.Domain.Enums;

namespace IS.IotMonitoring.CoreApi.Domain.Entities
{
    public class Device
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTimeOffset? LastMessageAt { get; private set; } = null;
        public DeviceType Type { get; private set; }
        public DeviceStatus Status { get; private set; }
        //parametreless constructor for EF Core
        private Device()
        {
            Name = string.Empty;
        }
        public static Device Create(string name, DeviceType type)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new Device
            { 
                Id = Guid.NewGuid(),
                Name = name,
                Status = DeviceStatus.Offline,
                Type = type,
            };
        }
        public void RecordHeartbeat()
        {
            LastMessageAt = DateTimeOffset.UtcNow;
            if(Status == DeviceStatus.Offline)
                Status = DeviceStatus.Online;
        }
        public TelemetryReading RecordTelemetry(double reading, string unit, DateTimeOffset timestamp)
        {
            if (Status == DeviceStatus.Deleted)
                throw new InvalidOperationException("Cannot record telemetry for a deleted device.");

            RecordHeartbeat();

            return TelemetryReading.Create(Id, reading, unit, timestamp);
        }
        public void GoOffline()
        {
            if (Status == DeviceStatus.Deleted)
                throw new InvalidOperationException("Device is already deleted.");
            Status = DeviceStatus.Offline;
        }
        public void Delete()
        {
            Status = DeviceStatus.Deleted;
        }
    }
}
