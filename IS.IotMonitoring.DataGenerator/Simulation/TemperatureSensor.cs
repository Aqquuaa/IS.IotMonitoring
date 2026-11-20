namespace IS.IotMonitoring.DataGenerator.Simulation
{
    public class TemperatureSensor
    {
        private const double OverheatTemperature = 45;
        private const double Smoothing = 0.3;
        private const double NoiseAmplitude = 0.3;

        private readonly double baseline;
        private DateTimeOffset overheatUntil = DateTimeOffset.MinValue;
        private double temperature;

        public Guid Id { get; }
        public string Name { get; }

        public TemperatureSensor(Guid id, string name)
        {
            Id = id;
            Name = name;
            baseline = 20 + Random.Shared.NextDouble() * 4;
            temperature = baseline;
        }

        public bool IsOverheating(DateTimeOffset now) => now < overheatUntil;

        public void StartOverheating(DateTimeOffset until) => overheatUntil = until;

        // Moves gradually towards the target (baseline, or 45 °C while overheating) with a bit of noise.
        public double NextReading(DateTimeOffset now)
        {
            var target = IsOverheating(now) ? OverheatTemperature : baseline;
            var noise = (Random.Shared.NextDouble() * 2 - 1) * NoiseAmplitude;
            temperature += (target - temperature) * Smoothing + noise;
            return Math.Round(temperature, 2);
        }
    }
}
