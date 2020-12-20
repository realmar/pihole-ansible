using System.Runtime.Serialization;

namespace HomeController.Processors.Input.Models
{
    [DataContract]
    public class SingleValueModel
    {
        [DataMember(Name = "value")]
        public double Value { get; init; }
    }
}
