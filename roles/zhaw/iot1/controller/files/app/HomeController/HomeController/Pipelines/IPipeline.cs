using MQTTnet.Protocol;
using System.Threading.Tasks;

namespace HomeController.Pipelines
{
    public interface IPipeline
    {
        string Topic { get; }
        MqttQualityOfServiceLevel QualityOfService { get; }
        Task Run(string topic, string message);
    }
}
