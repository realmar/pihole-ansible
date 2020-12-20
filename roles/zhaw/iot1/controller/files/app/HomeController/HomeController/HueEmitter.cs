using HomeController.Processors.Input.Models;
using Microsoft.Extensions.Hosting;
using MQTTnet.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeController
{
    internal class HueEmitter : BackgroundService
    {
        private readonly MqttService _mqttService;
        private readonly HueService _hueService;

        public HueEmitter(MqttService mqttService, HueService hueService)
        {
            _mqttService = mqttService;
            _hueService = hueService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);

                foreach (var light in await _hueService.GetLightsAsync().ConfigureAwait(false))
                {
                    var state = light.State;
                    var hueLight = new HueLightModel(
                        new Brightness(state.Brightness, light.Capabilities.Control.MaxLumen ?? default),
                        new ReciprocalMegakelvin(state.ColorTemperature ?? default, light.Capabilities.Control.ColorTemperature.Min, light.Capabilities.Control.ColorTemperature.Max),
                        state.Hue ?? default,
                        state.Saturation ?? default,
                        light.State.On);

                    var location = light.Name switch
                    {
                        "Kitchen 1" => "kitchen",
                        "Kitchen 2" => "kitchen",
                        "Main Door" => "maindoor",
                        "Hue ambiance panel 1" => "workplace",
                        "Hue ambiance panel 2" => "workplace",
                        _ => "LivingRoom"
                    };

                    await _mqttService.Publish(
                        $"iot1/hue/{location}",
                        MqttQualityOfServiceLevel.AtMostOnce,
                        hueLight, stoppingToken).ConfigureAwait(false);
                }
            }
        }
    }
}
