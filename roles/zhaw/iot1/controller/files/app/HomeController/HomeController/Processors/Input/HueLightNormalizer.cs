using HomeController.Processors.Input.Models;
using HomeController.Processors.Output.Models;

namespace HomeController.Processors.Input
{
    public class HueLightNormalizer : INormalizer<HueLightModel, LampModel>
    {
        public LampModel Process(in Context<HueLightModel> input)
        {
            return new()
            {
                Lumens = input.Model.Brightness.Value / 255d * input.Model.Brightness.MaxLumens,

                // https://en.wikipedia.org/wiki/Mired
                Kelvin = 1_000_000 / input.Model.MK1.Value,

                Hue = input.Model.Hue,
                Saturation = input.Model.Saturation,

                On = input.Model.On ? 1 : 0
            };
        }
    }
}
