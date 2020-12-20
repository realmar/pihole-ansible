using InfluxDB.Client.Core;

namespace HomeController.Processors.Output.Models
{
    [Measurement("lamp")]
    public record LampModel : BaseModel
    {
        [Column("lumens")]
        public double Lumens { get; init; }

        [Column("kelvin")]
        public int Kelvin { get; init; }

        [Column("hue")]
        public int Hue { get; init; }

        [Column("saturation")]
        public int Saturation { get; init; }

        [Column("on")]
        public int On { get; init; }
    }
}
