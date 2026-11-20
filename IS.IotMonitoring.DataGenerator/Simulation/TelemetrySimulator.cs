using IS.IotMonitoring.DataGenerator.Api;
using IS.IotMonitoring.DataGenerator.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IS.IotMonitoring.DataGenerator.Simulation
{
    public class TelemetrySimulator : BackgroundService
    {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        private readonly IotApiClient apiClient;
        private readonly GeneratorOptions options;
        private readonly ILogger<TelemetrySimulator> logger;

        public TelemetrySimulator(IotApiClient apiClient, IOptions<GeneratorOptions> options, ILogger<TelemetrySimulator> logger)
        {
            this.apiClient = apiClient;
            this.options = options.Value;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sensors = await RegisterSensorsAsync(stoppingToken);
            var nextOverheatAt = DateTimeOffset.UtcNow.AddSeconds(options.OverheatEverySeconds);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.IntervalSeconds));
            do
            {
                var now = DateTimeOffset.UtcNow;
                if (now >= nextOverheatAt)
                {
                    StartOverheating(sensors, now);
                    nextOverheatAt = now.AddSeconds(options.OverheatEverySeconds);
                }

                foreach (var sensor in sensors)
                    await SendReadingAsync(sensor, now, stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task<List<TemperatureSensor>> RegisterSensorsAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    var existing = await apiClient.GetActiveTemperatureSensorsAsync(stoppingToken);
                    var sensors = new List<TemperatureSensor>();

                    for (var i = 1; i <= options.SensorCount; i++)
                    {
                        var name = $"temp-sensor-{i:D2}";
                        var device = existing.FirstOrDefault(d => d.Name == name)
                            ?? await apiClient.CreateTemperatureSensorAsync(name, stoppingToken);
                        sensors.Add(new TemperatureSensor(device.Id, device.Name));
                    }

                    logger.LogInformation("Simulating {SensorCount} temperature sensors: {SensorNames}",
                        sensors.Count, string.Join(", ", sensors.Select(s => s.Name)));
                    return sensors;
                }
                catch (Exception ex) when (IsTransient(ex, stoppingToken))
                {
                    logger.LogWarning("Services are not reachable yet ({Error}). Retrying in {RetryDelay} s",
                        ex.Message, RetryDelay.TotalSeconds);
                    await Task.Delay(RetryDelay, stoppingToken);
                }
            }
        }

        private void StartOverheating(List<TemperatureSensor> sensors, DateTimeOffset now)
        {
            var sensor = sensors[Random.Shared.Next(sensors.Count)];
            sensor.StartOverheating(now.AddSeconds(options.OverheatDurationSeconds));

            logger.LogWarning("Sensor {SensorName} starts overheating for {DurationSeconds} s",
                sensor.Name, options.OverheatDurationSeconds);
        }

        private async Task SendReadingAsync(TemperatureSensor sensor, DateTimeOffset now, CancellationToken stoppingToken)
        {
            var reading = sensor.NextReading(now);
            try
            {
                await apiClient.SendReadingAsync(sensor.Id, reading, now, stoppingToken);
                logger.LogDebug("{SensorName}: {Reading} °C", sensor.Name, reading);
            }
            catch (Exception ex) when (IsTransient(ex, stoppingToken))
            {
                logger.LogWarning("Failed to send reading of {SensorName}: {Error}", sensor.Name, ex.Message);
            }
        }

        private static bool IsTransient(Exception ex, CancellationToken stoppingToken) =>
            !stoppingToken.IsCancellationRequested && ex is HttpRequestException or TaskCanceledException;
    }
}
