using HomeController.Processors.Input.Models;
using HomeController.Processors.Output.Models;

namespace HomeController.Processors.Input
{
    public class MicrophoneNormalizer : INormalizer<MicrophoneModel, AmbientVitalsModel>
    {
        public AmbientVitalsModel Process(in Context<MicrophoneModel> input)
        {
            return new()
            {
                NoiseDecibels = input.Model.Decibels
            };
        }
    }
}
