namespace IS.IotMonitoring.CoreApi.Domain.Entities
{
    public class TelemetryReading
    {
        public Guid Id { get; private set; }
        public Guid DeviceId { get; private set;  }
        public double Reading { get; private set;  }
        public string Unit { get; private set; } = string.Empty;
        public DateTimeOffset Timestamp { get; private set; }
        //constructor for mongodb
        private TelemetryReading() { }
        internal static TelemetryReading Create(Guid deviceId, double reading, string unit, DateTimeOffset timestamp)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(unit);

            return new TelemetryReading
            {
                Id = Guid.NewGuid(),
                DeviceId = deviceId,
                Reading = reading,
                Unit = unit,
                Timestamp = timestamp
            };
        }

    }
}
