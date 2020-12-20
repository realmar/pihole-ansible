using InfluxDB.Client.Core;

namespace HomeController.Processors.Output.Models
{
    [Measurement("light")]
    public record LightModel : BaseModel
    {
        [Column("lux")]
        public int Lux { get; init; }
    }
}
