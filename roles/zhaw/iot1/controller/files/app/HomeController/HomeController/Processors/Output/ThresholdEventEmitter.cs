using System;
using System.Threading.Tasks;

namespace HomeController.Processors.Output
{
    public class ThresholdEventEmitter<T> : ITerminal<T>
    {
        private readonly IEventSender _sender;
        private readonly Func<T, string> _messageFormatter;
        private readonly Predicate<T> _shouldEmit;

        public ThresholdEventEmitter(IEventSender sender, Func<T, string> messageFormatter, Predicate<T> shouldEmit)
        {
            _sender = sender;
            _messageFormatter = messageFormatter;
            _shouldEmit = shouldEmit;
        }

        public async Task Process(Context<T> input)
        {
            if (_shouldEmit.Invoke(input.Model))
            {
                await _sender.Send($"{input.Topic}\n\n{_messageFormatter.Invoke(input.Model)}", input.Model).ConfigureAwait(false);
            }
        }
    }
}
