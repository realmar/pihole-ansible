using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HomeController.Processors.Output
{
    public class TelegramEventSender : IEventSender
    {
        private readonly TelegramBotClient _client;
        private readonly string _chatId;

        public TelegramEventSender()
        {
            var json = File.ReadAllText("telegram_config.json");
            var config = JsonConvert.DeserializeObject<Config>(json);

            _client = new TelegramBotClient(config.Key ?? throw new NullReferenceException($"Telegram Key cannot be null {json}"));
            _chatId = config.ClientId ?? throw new NullReferenceException($"Telegram ChatId cannot be null {json}");
        }

        public async Task Send<T>(string message, T data)
        {
            var result = await _client.SendTextMessageAsync(_chatId, $"{message}\n\n{JsonConvert.SerializeObject(data)}").ConfigureAwait(false);
        }

        [DataContract]
        private struct Config
        {
            [DataMember(Name = "key")]
            public string Key;

            [DataMember(Name = "chat_id")]
            public string ClientId;
        }
    }
}
