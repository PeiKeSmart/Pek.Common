using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Pek.Configs.Interfaces;

namespace Pek.Configuration.Configuration
{
    public class FileConfigurationProvider : IConfigurationProvider
    {
        private readonly string _filePath;
        private readonly ConcurrentDictionary<string, string> _settings;
        private FileSystemWatcher _fileWatcher;

        public FileConfigurationProvider(string filePath)
        {
            _filePath = filePath;
            _settings = new ConcurrentDictionary<string, string>();
            LoadSettings();
            StartFileWatcher();
        }

        private void LoadSettings()
        {
            if (!File.Exists(_filePath))
                CreateDefaultFile();

            var json = File.ReadAllText(_filePath);
            var settings = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(json);

            foreach (var setting in settings)
                _settings[setting.Key] = setting.Value;
        }

        private void CreateDefaultFile()
        {
            var defaultSettings = new ConcurrentDictionary<string, string>
            {
                ["DefaultSetting1"] = "DefaultValue1",
                ["DefaultSetting2"] = "DefaultValue2"
            };

            var json = JsonSerializer.Serialize(defaultSettings);
            File.WriteAllText(_filePath, json);
        }

        private void StartFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(_filePath))
            {
                Filter = Path.GetFileName(_filePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.EnableRaisingEvents = true;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Delay to ensure the file is fully written
            Thread.Sleep(100);
            LoadSettings();
        }

        public string Get(string key)
        {
            return _settings.TryGetValue(key, out var value) ? value : null;
        }

        public void Set(string key, string value)
        {
            _settings[key] = value;
            SaveSettings();
        }

        private void SaveSettings()
        {
            var json = JsonSerializer.Serialize(_settings);
            File.WriteAllText(_filePath, json);
        }
    }
}