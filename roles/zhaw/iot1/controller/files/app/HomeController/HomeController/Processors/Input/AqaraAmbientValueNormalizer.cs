using HomeController.Processors.Input.Models;
using HomeController.Processors.Output.Models;

namespace HomeController.Processors.Input
{
    public class AqaraAmbientValueNormalizer : INormalizer<AqaraTemperatureHumiditySensorModel, AmbientVitalsModel>
    {
        public AmbientVitalsModel Process(in Context<AqaraTemperatureHumiditySensorModel> context) =>
            new()
            {
                TemperatureCelsius = context.Model.Temperature,
                HumidityPercentage = context.Model.Humidity,
                PressureHectopascal = context.Model.Pressure,
            };
    }
}
