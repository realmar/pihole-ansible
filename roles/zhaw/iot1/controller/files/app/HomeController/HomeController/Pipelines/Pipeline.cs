using HomeController.Processors;
using HomeController.Processors.Output.Models;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeController.Pipelines
{
    public class Pipeline<TIn, TOut> : IPipeline
        where TIn : class
        where TOut : BaseModel
    {
        private readonly INormalizer<TIn, TOut> _normalizer;
        private readonly IReadOnlyList<IMiddleware<TOut>> _middlewares;
        private readonly IReadOnlyList<ITerminal<TOut>> _terminals;
        private readonly ILogger _logger;

        public Pipeline(
            string topic,
            MqttQualityOfServiceLevel qualityOfService,
            INormalizer<TIn, TOut> normalizer,
            IReadOnlyList<IMiddleware<TOut>> middlewares,
            IReadOnlyList<ITerminal<TOut>> terminals,
            ILogger logger)
        {
            Topic = topic;
            QualityOfService = qualityOfService;
            _normalizer = normalizer;
            _middlewares = middlewares;
            _terminals = terminals;
            _logger = logger;
        }

        public string Topic { get; }

        public MqttQualityOfServiceLevel QualityOfService { get; }

        public async Task Run(string topic, string message)
        {
            TIn model;
            if (typeof(TIn) == typeof(string))
            {
                model = (TIn) Convert.ChangeType(message, typeof(string));
            }
            else
            {
                model = JsonConvert.DeserializeObject<TIn>(message);
            }

            var normalized = _normalizer.Process(Context<TIn>.Create(topic, model));

            if (normalized != null)
            {
                var finalModel = normalized;
                foreach (var middleware in _middlewares)
                {
                    finalModel = await middleware.Process(Context<TOut>.Create(topic, finalModel));
                }

                foreach (var terminal in _terminals)
                {
                    await terminal.Process(Context<TOut>.Create(topic, finalModel)).ConfigureAwait(false);
                }
            }
            else
            {
                _logger.LogDebug($"Processor {_normalizer.GetType().Name} discarded measurement from topic {topic}");
            }
        }
    }
}
