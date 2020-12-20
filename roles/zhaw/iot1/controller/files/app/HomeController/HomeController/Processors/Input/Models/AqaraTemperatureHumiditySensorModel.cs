using System.Runtime.Serialization;

namespace HomeController.Processors.Input.Models
{
    [DataContract]
    public record AqaraTemperatureHumiditySensorModel
    {
        [DataMember(Name = "battery")]
        public double Battery { get; init; }

        [DataMember(Name = "humidity")]
        public double Humidity { get; init; }

        [DataMember(Name = "linkquality")]
        public double LinkQuality { get; init; }

        [DataMember(Name = "pressure")]
        public double Pressure { get; init; }

        [DataMember(Name = "temperature")]
        public double Temperature { get; init; }

        [DataMember(Name = "voltage")]
        public int Voltage { get; init; }
    }
}
