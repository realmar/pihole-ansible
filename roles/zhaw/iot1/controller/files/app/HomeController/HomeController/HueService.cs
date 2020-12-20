using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HomeController
{
    internal class HueService : IHostedService
    {
        private readonly string _hueIp;
        private readonly ILogger<HueService> _logger;
        private IHueClient _client;

        public HueService(IConfiguration config, ILogger<HueService> logger)
        {
            _logger = logger;
            _hueIp = config.GetValue<string>("PhillipsHueIp");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            const string keyFile = "hue_app_key.txt";
            string authKey;

            var client = new LocalHueClient(_hueIp);

            if (File.Exists(keyFile))
            {
                authKey = (await File.ReadAllTextAsync(keyFile, cancellationToken).ConfigureAwait(false)).Trim();
                client.Initialize(authKey);
            }
            else
            {
                for (var i = 10 - 1; i >= 0; i--)
                {
                    _logger.LogInformation($"Press hue button: {i + 1}");
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                }

                authKey = await client.RegisterAsync("iot1_home_controller", Dns.GetHostName()).ConfigureAwait(false);

                if (authKey == null)
                {
                    throw new IOException("Failed to retrieve Hue secret key");
                }
                else
                {
                    await File.WriteAllTextAsync(keyFile, authKey, cancellationToken).ConfigureAwait(false);
                }
            }

            _client = client;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Light>> GetLightsAsync() => _client.GetLightsAsync();

        public Task SendCommand(LightCommand command) => _client.SendCommandAsync(command);
    }
}
