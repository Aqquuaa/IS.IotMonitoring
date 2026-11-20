using System.ComponentModel.DataAnnotations;

namespace IS.IotMonitoring.DataGenerator.Options
{
    public class GeneratorOptions
    {
        public const string SectionName = "Generator";

        [Required, Url]
        public string AuthServiceUrl { get; set; } = string.Empty;

        [Required, Url]
        public string CoreApiUrl { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Range(1, 50)]
        public int SensorCount { get; set; } = 3;

        [Range(1, 3600)]
        public int IntervalSeconds { get; set; } = 2;

        [Range(5, 3600)]
        public int OverheatEverySeconds { get; set; } = 60;

        [Range(1, 3600)]
        public int OverheatDurationSeconds { get; set; } = 30;
    }
}
