using HomeController.Processors.Output.Models;
using System;

namespace HomeController.Processors.Input
{
    public class DetectCountNormalizer<TIn> : INormalizer<TIn, UsageModel>
    {
        private readonly Predicate<TIn> _activePredicate;

        public DetectCountNormalizer(Predicate<TIn> activePredicate)
        {
            _activePredicate = activePredicate;
        }

        public UsageModel Process(in Context<TIn> input)
        {
            return new UsageModel
            {
                CurrentState = _activePredicate.Invoke(input.Model) ? UsageModel.State.Active : UsageModel.State.InActive
            };
        }
    }
}
