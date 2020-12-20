using System.Threading.Tasks;

namespace HomeController.Processors.Output
{
    public interface IEventSender
    {
        Task Send<T>(string message, T data);
    }
}
