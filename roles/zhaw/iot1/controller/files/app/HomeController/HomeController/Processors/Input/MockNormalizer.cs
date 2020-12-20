using Microsoft.Extensions.Logging;

namespace HomeController.Processors.Input
{
    public class MockNormalizer : INormalizer<string, string>
    {
        private readonly ILogger<MockNormalizer> _logger;

        public MockNormalizer(ILogger<MockNormalizer> logger)
        {
            _logger = logger;
        }

        public string Process(in Context<string> input)
        {
            return input.Model;
        }
    }
}
