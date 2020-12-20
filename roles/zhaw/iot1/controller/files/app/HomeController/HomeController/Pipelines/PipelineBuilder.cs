using HomeController.Processors;
using HomeController.Processors.Middlewares;
using HomeController.Processors.Output;
using HomeController.Processors.Output.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeController.Pipelines
{
    public class PipelineBuilder<TIn, TOut>
        where TIn : class
        where TOut : BaseModel
    {
        private readonly IServiceProvider _provider;
        private INormalizer<TIn, TOut> _normalizer;
        private List<IMiddleware<TOut>> _middlewares = new();
        private List<ITerminal<TOut>> _terminals = new();

        private PipelineBuilder(IServiceProvider provider)
        {
            _provider = provider;
        }

        public static PipelineBuilder<TIn, TOut> Create(IServiceProvider provider) => new(provider);

        public static PipelineBuilder<TIn, TOut> CreateWithDefaults<TNormalizer>(IServiceProvider provider, TNormalizer normalizer = null)
            where TNormalizer : class, INormalizer<TIn, TOut> =>
            Create(provider)
                .WithNormalizer(normalizer ?? provider.GetService<TNormalizer>())
                .AddMiddleware(provider.GetService<CommonDataProcessor<TOut>>())
                .AddMiddleware(provider.GetService<TopicToLocationProcessor<TOut>>())
                .AddTerminal(provider.GetService<InfluxDbPersistence<TOut>>());

        public PipelineBuilder<TIn, TOut> WithNormalizer(INormalizer<TIn, TOut> n)
        {
            _normalizer = n;
            return this;
        }


        public PipelineBuilder<TIn, TOut> WithDebouncing(Func<IReadOnlyList<TOut>, TOut> reducer, Predicate<TOut> preventDebouncing = null)
        {
            var debouncer = new Debouncer<TOut>(
                _terminals.ToList(),
                TimeSpan.FromSeconds(_provider.GetService<IConfiguration>().GetValue<int>("DebounceAvgSeconds")),
                reducer,
                preventDebouncing);

            _terminals.Clear();
            _terminals.Add(debouncer);

            return this;
        }

        public PipelineBuilder<TIn, TOut> AddAlert(Predicate<TOut> shouldEmit, Func<TOut, string> messageFormatter) =>
            AddTerminal(new ThresholdEventEmitter<TOut>(_provider.GetService<IEventSender>(), messageFormatter, shouldEmit));

        public PipelineBuilder<TIn, TOut> AddMiddleware<TMiddleware>()
            where TMiddleware : IMiddleware<TOut>
        {
            AddMiddleware(_provider.GetService<TMiddleware>());
            return this;
        }

        public PipelineBuilder<TIn, TOut> AddMiddleware(IMiddleware<TOut> m)
        {
            _middlewares.Add(m);
            return this;
        }

        public PipelineBuilder<TIn, TOut> AddTerminal(ITerminal<TOut> t)
        {
            _terminals.Add(t);
            return this;
        }

        public IPipeline Build(string topic, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce) =>
            new Pipeline<TIn, TOut>(topic, qos, _normalizer, _middlewares, _terminals, _provider.GetService<ILogger<IPipeline>>());
    }
}
