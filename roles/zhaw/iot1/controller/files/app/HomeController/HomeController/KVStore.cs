using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HomeController
{
    public class KVStore
    {
        private readonly ILogger<KVStore> _logger;
        private const string DBPath = "KVStore.json";
        private readonly Dictionary<string, string> _store = new();
        private readonly SemaphoreSlim _mutex = new(1);

        public KVStore(ILogger<KVStore> logger)
        {
            _logger = logger;

            if (File.Exists(DBPath))
            {
                try
                {
                    _store = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(DBPath)) ?? new();
                }
                catch (Exception e)
                {
                    logger.LogWarning($"Failed to load KVStore: {e}");
                }
            }
        }

        public async Task Store<T>(string key, T data)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                var json = JsonConvert.SerializeObject(data);
                _store[key] = json;

                await SchdooooreDatSchitte().ConfigureAwait(false);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async Task<(bool Exists, T Value)> Get<T>(string key)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_store.TryGetValue(key, out var value))
                {
                    try
                    {
                        return (true, JsonConvert.DeserializeObject<T>(value));
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning($"Failed to deserialize value: {e}");
                        return (false, default);
                    }
                }
                else
                {
                    return (false, default);
                }
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async Task Yeet(string key)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_store.ContainsKey(key))
                {
                    _store.Remove(key);
                    await SchdooooreDatSchitte().ConfigureAwait(false);
                }
            }
            finally
            {
                _mutex.Release();
            }
        }

        private async Task SchdooooreDatSchitte()
        {
            await File.WriteAllTextAsync(DBPath, JsonConvert.SerializeObject(_store, Formatting.Indented)).ConfigureAwait(false);
        }
    }
}
