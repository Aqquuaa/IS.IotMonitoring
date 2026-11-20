using IS.IotMonitoring.CoreApi.Domain.Entities;
using IS.IotMonitoring.CoreApi.Domain.Enums;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace IS.IotMonitoring.CoreApi.Application.Metrics
{
    public class TelemetryMetrics
    {
        public const string MeterName = "IS.IotMonitoring.CoreApi";

        private static readonly Meter meter = new(MeterName);

        private readonly Counter<long> readings =
            meter.CreateCounter<long>("iot.telemetry.readings", description: "Telemetry readings recorded");

        private readonly Gauge<double> temperature =
            meter.CreateGauge<double>("iot.device.temperature.celsius", description: "Latest temperature reported by a device");

        public void RecordReading(Device device, double reading)
        {
            var tags = new TagList
            {
                { "device_id", device.Id.ToString() },
                { "device_name", device.Name },
                { "device_type", device.Type.ToString() }
            };

            readings.Add(1, tags);

            if (device.Type == DeviceType.TemperatureSensor)
                temperature.Record(reading, tags);
        }
    }
}
