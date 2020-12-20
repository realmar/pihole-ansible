using System.Threading.Tasks;
using HomeController.Processors.Output.Models;

namespace HomeController.Processors.Middlewares
{
    public class SensorToLocationProcessor<TModel> : IMiddleware<TModel>
        where TModel : BaseModel
    {
        public Task<TModel> Process(Context<TModel> context)
        {
            return Task.FromResult(context.Model with { Location = SensorNameToLocation(context.Model.Sensor) });
        }

        private static string SensorNameToLocation(string sensorName) =>
            sensorName switch
            {
                "aqara-temperature-humidity-sensor-01" => "LivingRoom",
                _ => sensorName
            };
    }
}
