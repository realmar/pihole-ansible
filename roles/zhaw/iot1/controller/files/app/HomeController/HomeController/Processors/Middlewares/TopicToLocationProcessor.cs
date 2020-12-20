using System.Threading.Tasks;
using HomeController.Processors.Output.Models;

namespace HomeController.Processors.Middlewares
{
    public class TopicToLocationProcessor<TModel> : IMiddleware<TModel>
        where TModel : BaseModel
    {
        public Task<TModel> Process(Context<TModel> context)
        {
            string location;
            var parts = context.Topic.Split('/');

            if (parts.Length < 3)
            {
                location = "Unknown";
            }
            else
            {
                location = parts[2];
            }

            return Task.FromResult(context.Model with { Location = location });
        }
    }
}
