using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace Pek.Configuration.Configuration
{
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly FileConfigurationProvider _fileProvider;
        private readonly ConcurrentDictionary<string, object> _configurations;

        public ConfigurationManager(string filePath)
        {
            _fileProvider = new FileConfigurationProvider(filePath);
            _configurations = new ConcurrentDictionary<string, object>();
            LoadConfigurations();
            StartFileWatcher();
        }

        private void LoadConfigurations()
        {
            var configData = _fileProvider.Load();
            foreach (var kvp in configData)
                _configurations.TryAdd(kvp.Key, kvp.Value);
        }

        private void StartFileWatcher()
        {
            _fileProvider.FileChanged += (sender, args) => LoadConfigurations();
        }

        public T Get<T>(string key)
        {
            if (_configurations.TryGetValue(key, out var value))
                return (T)value;
            return default;
        }

        public void Set<T>(string key, T value)
        {
            _configurations[key] = value;
            _fileProvider.Save(_configurations);
        }
    }
}