using System;
using System.Threading.Tasks;
using HomeController.Processors.Output.Models;
using Microsoft.Extensions.Logging;

namespace HomeController.Processors.Middlewares
{
    public class UsageCalculatorMiddleware : IMiddleware<UsageModel>
    {
        private readonly TimeSpan _bounceTime = TimeSpan.FromSeconds(6);
        private readonly KVStore _store;
        private readonly ILogger<UsageCalculatorMiddleware> _logger;

        public UsageCalculatorMiddleware(KVStore store, ILogger<UsageCalculatorMiddleware> logger)
        {
            _store = store;
            _logger = logger;
        }

        public async Task<UsageModel> Process(Context<UsageModel> input)
        {
            var (exists, data) = await _store.Get<Data>(input.Topic).ConfigureAwait(false);
            if (exists)
            {
                if (data.InitState != input.Model.CurrentState)
                {
                    if (data.LastChange == null)
                    {
                        data.LastChange = DateTime.UtcNow;
                    }
                    else
                    {
                        if (DateTime.UtcNow - data.LastChange > _bounceTime)
                        {
                            _logger.LogInformation($"Changed state: {input.Topic} start: {data.Start} transition: {data.InitState} --> {input.Model.CurrentState}");

                            await _store.Yeet(input.Topic).ConfigureAwait(false);

                            return input.Model with
                                {
                                JustGotActivated = input.Model.CurrentState == UsageModel.State.Active,
                                Duration = DateTime.UtcNow - data.Start,
                                };
                        }
                    }
                }
                else
                {
                    data.LastChange = null;
                }
            }
            else
            {
                data = new Data
                {
                    Start = DateTime.UtcNow,
                    InitState = input.Model.CurrentState
                };
            }

            await _store.Store(input.Topic, data).ConfigureAwait(false);

            return input.Model;
        }

        [Serializable]
        private class Data
        {
            public DateTime Start;
            public DateTime? LastChange;
            public UsageModel.State InitState;
        }
    }
}
