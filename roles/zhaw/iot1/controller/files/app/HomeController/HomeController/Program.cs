using HomeController.Pipelines;
using HomeController.Processors.Input;
using HomeController.Processors.Input.Models;
using HomeController.Processors.Middlewares;
using HomeController.Processors.Output;
using HomeController.Processors.Output.Models;
using HomeController.Utils;
using InfluxDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeController
{
    internal static class Program
    {
        internal static Task Main() =>
            CreateHostBuilder().Build().RunAsync();

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json"))
                .ConfigureServices(services => services
                    .AddSingleton<MqttService>()
                    .AddHostedService(provider => provider.GetService<MqttService>())
                    .AddSingleton<HueService>()
                    .AddHostedService(provider => provider.GetService<HueService>())
                    .AddHostedService<HueEmitter>()
                    .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information))
                    .AddSingleton<KVStore>()
                    .AddSingleton<TopicChecker>()
                    .AddTransient<AqaraAmbientValueNormalizer>()
                    .AddTransient<LightNormalizer>()
                    .AddTransient<HueLightNormalizer>()
                    .AddTransient<MicrophoneNormalizer>()
                    .AddTransient(typeof(SensorToLocationProcessor<>))
                    .AddTransient(typeof(TopicToLocationProcessor<>))
                    .AddTransient(typeof(CommonDataProcessor<>))
                    .AddTransient<UsageCalculatorMiddleware>()
                    .AddTransient(typeof(InfluxDbPersistence<>))
                    .AddSingleton<TelegramEventSender>()
                    .AddSingleton<IEventSender>(provider => provider.GetService<TelegramEventSender>())
                    .AddTransient(provider =>
                    {
                        static LightModel LightReducer(IReadOnlyList<LightModel> list) =>
                            list[^1] with { Lux = (int) list.Average(model => model.Lux) };

                        static UsageModel UsageReducer(IReadOnlyList<UsageModel> list) =>
                            list.Count(model => model.CurrentState == UsageModel.State.Active) / (double) list.Count >= 0.5d
                                ? list[^1] with { CurrentState = UsageModel.State.Active }
                                : list[^1] with { CurrentState = UsageModel.State.InActive };

                        return PipelineCollection.Create()

                            // zigbee2mqtt
                            .Add(PipelineBuilder<AqaraTemperatureHumiditySensorModel, AmbientVitalsModel>
                                .CreateWithDefaults<AqaraAmbientValueNormalizer>(provider)
                                .AddMiddleware(provider.GetService<SensorToLocationProcessor<AmbientVitalsModel>>())
                                .AddAlert(model => model.HumidityPercentage != null && model.HumidityPercentage <= 46d, model => $"Humidifier run out of water! Humidity: {model.HumidityPercentage}%")
                                .AddAlert(model => model.TemperatureCelsius != null && model.TemperatureCelsius <= 22d, model => $"It's too cold, turn on the heater. Temperature: {model.TemperatureCelsius}°C")
                                .Build("zigbee2mqtt/+"))

                            // light
                            .Add(PipelineBuilder<SingleValueModel, LightModel>
                                .CreateWithDefaults<LightNormalizer>(provider)
                                .AddTerminal(new LightAdjuster(provider.GetService<HueService>(), new List<(Range, int)>
                                {
                                    (..600, 233),
                                    (720..980, 156),
                                }))
                                .WithDebouncing(LightReducer)
                                .Build("iot1/light/+"))

                            // usage
                            // bathroom is in use when the door is open
                            .Add(PipelineBuilder<SingleValueModel, UsageModel>
                                .CreateWithDefaults(provider, new DetectCountNormalizer<SingleValueModel>(model => model.Value >= 20d))
                                .AddMiddleware<UsageCalculatorMiddleware>()
                                .AddAlert(model => model.Deactivated != null && model.Deactivated == 1, model => $"Welcome back to your computer, you spent {model.DurationSeconds}s in the bathroom.")
                                .WithDebouncing(UsageReducer, model => model.JustGotActivated != null)
                                .Build("iot1/ultrasonic/bathroom", MqttQualityOfServiceLevel.ExactlyOnce))

                            // phillips hue
                            .Add(PipelineBuilder<HueLightModel, LampModel>
                                .CreateWithDefaults<HueLightNormalizer>(provider)
                                .Build("iot1/hue/+"))

                            // laptop microphone
                            .Add(PipelineBuilder<MicrophoneModel, AmbientVitalsModel>
                                .CreateWithDefaults<MicrophoneNormalizer>(provider)
                                .WithDebouncing(list => list[^1] with { NoiseDecibels = list.Where(model => model.NoiseDecibels != null).Average(model => model.NoiseDecibels)})
                                .AddAlert(model => model.NoiseDecibels != null && model.NoiseDecibels >= 60d, model => $"Stop screaming. Decibels: {model.NoiseDecibels}dB")
                                .Build("iot1/microphone/+"));
                    })
                    .AddSingleton(provider => InfluxDBClientFactory.Create(provider.GetService<IConfiguration>()["InfluxDBUrl"]))
                    .AddSingleton(provider => new MqttClientOptionsBuilder()
                        .WithClientId("iot1_controller")
                        .WithTcpServer(provider.GetService<IConfiguration>()["MQTTUrl"])
                        .Build())
                    .AddTransient(_ => new MqttFactory().CreateManagedMqttClient())
                    .AddScoped<MqttFactory>());
    }
}
