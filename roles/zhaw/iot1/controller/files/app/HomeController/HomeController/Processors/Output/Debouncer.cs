using HomeController.Processors.Output.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeController.Processors.Output
{
    public class Debouncer<TValue> : ITerminal<TValue>
        where TValue : BaseModel
    {
        private readonly TimeSpan _duration;
        private readonly Dictionary<string, List<TValue>> _items = new();
        private readonly IReadOnlyList<ITerminal<TValue>> _next;
        private readonly Func<IReadOnlyList<TValue>, TValue> _reducer;
        private readonly Predicate<TValue> _preventDebouncing;

        public Debouncer(IReadOnlyList<ITerminal<TValue>> next, TimeSpan duration, Func<IReadOnlyList<TValue>, TValue> reducer, Predicate<TValue> preventDebouncing)
        {
            _next = next;
            _duration = duration;
            _reducer = reducer;
            _preventDebouncing = preventDebouncing;
        }

        public Task Process(Context<TValue> input)
        {
            lock (_items)
            {
                if (_preventDebouncing?.Invoke(input.Model) ?? false)
                {
                    ProcessNext(input);
                    SendData(input.Topic, items => SendData(input, items));
                }
                else
                {
                    SendData(input.Topic, items =>
                        {
                            if (DateTime.UtcNow - items[0].DateTime > _duration)
                            {
                                SendData(input, items);
                            }
                            else
                            {
                                items.Add(input.Model);
                            }
                        },
                        () => _items[input.Topic] = new List<TValue> { input.Model });
                }
            }

            return Task.CompletedTask;
        }

        private void SendData(string topic, Action<List<TValue>> handler, Action elseHandler = null)
        {
            if (_items.TryGetValue(topic, out var items))
            {
                handler.Invoke(items);
            }
            else
            {
                elseHandler?.Invoke();
            }
        }

        private void SendData(Context<TValue> input, List<TValue> items)
        {
            var topic = input.Topic;
            var reduced = _reducer.Invoke(items);
            var context = Context<TValue>.Create(topic, reduced);

            ProcessNext(context);

            _items.Remove(input.Topic);
        }

        private void ProcessNext(Context<TValue> context)
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                foreach (var terminal in _next)
                {
                    await terminal.Process(context);
                }
            });
        }
    }
}
