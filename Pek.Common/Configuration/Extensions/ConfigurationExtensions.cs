using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace Pek.Configuration.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void LoadConfiguration<T>(this IConfigurationManager configManager, string filePath, T defaultConfig) where T : class
        {
            if (!File.Exists(filePath))
                SaveConfiguration(filePath, defaultConfig);
            else
            {
                var json = File.ReadAllText(filePath);
                var config = JsonSerializer.Deserialize<T>(json) ?? defaultConfig;
                configManager.SetConfiguration(config);
            }

            WatchFile(filePath, configManager);
        }

        public static void SaveConfiguration<T>(string filePath, T config) where T : class
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        private static void WatchFile(string filePath, IConfigurationManager configManager)
        {
            var fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(filePath))
            {
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            fileWatcher.Changed += (sender, e) =>
            {
                Thread.Sleep(100); // Delay to allow file write to complete
                var json = File.ReadAllText(filePath);
                var config = JsonSerializer.Deserialize<T>(json);
                configManager.SetConfiguration(config);
            };

            fileWatcher.EnableRaisingEvents = true;
        }
    }
}