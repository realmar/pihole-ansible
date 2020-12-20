using System.Threading.Tasks;

namespace HomeController.Processors
{
    public interface ITerminal<TModel>
    {
        Task Process(Context<TModel> input);
    }
}
