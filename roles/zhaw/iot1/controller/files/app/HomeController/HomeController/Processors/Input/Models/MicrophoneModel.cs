using System.Runtime.Serialization;

namespace HomeController.Processors.Input.Models
{
    [DataContract]
    public record MicrophoneModel
    {
        [DataMember]
        public double Decibels { get; init; }
    }
}
