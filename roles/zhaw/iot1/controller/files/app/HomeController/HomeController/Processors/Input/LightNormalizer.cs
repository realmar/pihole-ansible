using HomeController.Processors.Input.Models;
using HomeController.Processors.Output.Models;

namespace HomeController.Processors.Input
{
    public class LightNormalizer : INormalizer<SingleValueModel, LightModel>
    {
        public LightModel Process(in Context<SingleValueModel> context) =>
            new()
            {
                Lux = (int) context.Model.Value
            };
    }
}
