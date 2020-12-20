using System;
using InfluxDB.Client.Core;

namespace HomeController.Processors.Output.Models
{
    public record BaseModel
    {
        [Column(IsTimestamp = true)]
        public DateTime DateTime { get; init; }

        [Column("location", IsTag = true)]
        public string Location { get; init; }

        [Column("sensor", IsTag = true)]
        public string Sensor { get; init; }
    }
}
