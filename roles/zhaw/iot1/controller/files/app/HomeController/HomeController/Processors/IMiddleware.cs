using System.Threading.Tasks;

namespace HomeController.Processors
{
    public interface IMiddleware<TModel>
    {
        Task<TModel> Process(Context<TModel> input);
    }
}
