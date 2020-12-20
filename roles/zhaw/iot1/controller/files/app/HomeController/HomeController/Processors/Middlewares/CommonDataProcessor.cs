using System;
using System.Threading.Tasks;
using HomeController.Processors.Output.Models;

namespace HomeController.Processors.Middlewares
{
    public class CommonDataProcessor<TModel> : IMiddleware<TModel>
        where TModel : BaseModel
    {
        public Task<TModel> Process(Context<TModel> input)
        {
            string sensor;
            var parts = input.Topic.Split('/');

            if (parts.Length < 2)
            {
                sensor = "Unknown";
            }
            else
            {
                sensor = parts[1];
            }

            return Task.FromResult(input.Model with { DateTime = DateTime.UtcNow, Sensor = sensor });
        }
    }
}
