using HomeController.Processors.Output.Models;
using Q42.HueApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeController.Processors.Output
{
    internal class LightAdjuster : ITerminal<LightModel>
    {
        private int _currentTemperature;
        private readonly HueService _hue;
        private readonly List<(Range, int)> _thresholds;

        public LightAdjuster(HueService hue, List<(Range, int)> thresholds)
        {
            _hue = hue;
            _thresholds = thresholds;
        }

        public Task Process(Context<LightModel> input)
        {
            var lux = input.Model.Lux;
            foreach (var (range, temperature) in _thresholds)
            {
                if (lux >= range.Start.Value && lux <= range.End.Value)
                {
                    if (temperature != _currentTemperature)
                    {
                        _currentTemperature = temperature;
                        return _hue.SendCommand(new LightCommand { ColorTemperature = temperature });
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
