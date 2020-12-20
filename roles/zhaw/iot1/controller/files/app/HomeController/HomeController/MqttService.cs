using HomeController.Pipelines;
using HomeController.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeController
{
    internal class MqttService : IHostedService
    {
        private readonly IMqttClientOptions _options;
        private readonly IManagedMqttClient _client;
        private readonly PipelineCollection _pipelines;
        private readonly TopicChecker _topicChecker;
        private readonly ILogger _logger;

        public MqttService(IMqttClientOptions options, IManagedMqttClient client, PipelineCollection pipelines, TopicChecker topicChecker, ILogger<MqttService> logger)
        {
            _options = options;
            _client = client;
            _pipelines = pipelines;
            _topicChecker = topicChecker;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT client");
            _logger.LogInformation($"Using options: {JsonConvert.SerializeObject(_options)}");

            _client.UseApplicationMessageReceivedHandler(args =>
            {
                var message = args.ApplicationMessage;
                foreach (var pipeline in _pipelines)
                {
                    if (_topicChecker.Test(pipeline.Topic, message.Topic))
                    {
                        ThreadPool.QueueUserWorkItem(async _ => await ProcessMessage(pipeline, message));
                    }
                }
            });

            _client.UseConnectedHandler(_ =>
                _pipelines.ForEach(pipeline => _client.SubscribeAsync(
                    new MqttTopicFilterBuilder()
                        .WithTopic(pipeline.Topic)
                        .WithQualityOfServiceLevel(pipeline.QualityOfService)
                        .Build())));

            await _client.StartAsync(new ManagedMqttClientOptions
            {
                ClientOptions = _options
            });

            _logger.LogInformation("Started MQTT client");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT client");
            return _client.StopAsync();
        }

        public Task Publish<T>(string topic, MqttQualityOfServiceLevel qos, T message, CancellationToken cancellationToken = default)
        {
            var stringMessage = JsonConvert.SerializeObject(message);
            return _client.PublishAsync(builder => builder
                .WithTopic(topic)
                .WithQualityOfServiceLevel(qos)
                .WithPayload(stringMessage), cancellationToken);
        }

        private async Task ProcessMessage(IPipeline pipeline, MqttApplicationMessage message)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(message.Payload);
                _logger.LogDebug($"{message.Topic}: {payload}");

                await pipeline.Run(message.Topic, payload).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to run pipeline {pipeline.GetType().Name}");
            }
        }
    }
}
