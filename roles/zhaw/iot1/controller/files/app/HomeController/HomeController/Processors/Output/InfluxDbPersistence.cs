using HomeController.Processors.Output.Models;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HomeController.Processors.Output
{
    internal class InfluxDbPersistence<TModel> : ITerminal<TModel>
        where TModel : BaseModel
    {
        private readonly ILogger _logger;
        private readonly InfluxDBClient _influxDbClient;
        private readonly string _database;
        private readonly string _organization;

        public InfluxDbPersistence(IConfiguration config, InfluxDBClient influxDbClient, ILogger<InfluxDbPersistence<TModel>> logger)
        {
            _influxDbClient = influxDbClient;
            _logger = logger;

            _database = config["Persistence:Database"];
            _organization = config["Persistence:Organization"];
        }

        public async Task Process(Context<TModel> input)
        {
            var model = input.Model;

            _logger.LogInformation(JsonConvert.SerializeObject(input.Model));

            var api = _influxDbClient.GetWriteApiAsync();
            await api.WriteMeasurementAsync(_database, _organization, WritePrecision.Ms, model).ConfigureAwait(false);
        }
    }
}
