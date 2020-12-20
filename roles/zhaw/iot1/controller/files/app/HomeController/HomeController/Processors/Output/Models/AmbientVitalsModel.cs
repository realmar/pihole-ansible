using InfluxDB.Client.Core;

namespace HomeController.Processors.Output.Models
{
    [Measurement("ambient_vitals")]
    public record AmbientVitalsModel : BaseModel
    {
        [Column("temperature_c")]
        public double? TemperatureCelsius { get; init; }

        [Column("humidity_%")]
        public double? HumidityPercentage { get; init; }

        [Column("pressure_hpa")]
        public double? PressureHectopascal { get; init; }

        [Column("noise_db")]
        public double? NoiseDecibels { get; init; }
    }
}
