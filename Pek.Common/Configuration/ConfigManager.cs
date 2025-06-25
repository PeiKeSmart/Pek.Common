using System.Collections.Concurrent;
using System.Text.Json;

namespace Pek.Configuration;

/// <summary>
/// 配置管理器
/// </summary>
public static class ConfigManager
{
    private static readonly ConcurrentDictionary<Type, object> _configs = new();
    private static readonly ConcurrentDictionary<Type, JsonSerializerOptions> _serializerOptions = new();
    private static readonly ConcurrentDictionary<Type, string> _configFileNames = new();

    /// <summary>
    /// 注册配置类型
    /// </summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    /// <param name="serializerOptions">序列化选项</param>
    /// <param name="fileName">配置文件名（可选）</param>
    public static void RegisterConfig<TConfig>(JsonSerializerOptions serializerOptions, string? fileName = null)
        where TConfig : Config, new()
    {
        var configType = typeof(TConfig);
        _serializerOptions[configType] = serializerOptions;
        _configFileNames[configType] = fileName ?? configType.Name;
    }

    /// <summary>
    /// 获取配置实例
    /// </summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    /// <param name="forceReload">是否强制重新加载</param>
    /// <returns></returns>
    public static TConfig GetConfig<TConfig>(bool forceReload = false) where TConfig : Config, new()
    {
        var configType = typeof(TConfig);

        if (!forceReload && _configs.TryGetValue(configType, out var cachedConfig))
        {
            return (TConfig)cachedConfig;
        }

        var config = LoadConfig<TConfig>();
        _configs[configType] = config;
        return config;
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="config">配置实例</param>
    public static void SaveConfig(Config config)
    {
        var configType = config.GetType();

        if (!_serializerOptions.TryGetValue(configType, out var options))
        {
            throw new InvalidOperationException($"配置类型 {configType.Name} 未注册序列化选项");
        }

        try
        {
            var filePath = GetConfigFilePath(configType);
            var json = JsonSerializer.Serialize(config, configType, options);
            File.WriteAllText(filePath, json);

            // 更新缓存
            _configs[configType] = config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存配置文件失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    private static TConfig LoadConfig<TConfig>() where TConfig : Config, new()
    {
        var configType = typeof(TConfig);

        if (!_serializerOptions.TryGetValue(configType, out var options))
        {
            throw new InvalidOperationException($"配置类型 {configType.Name} 未注册序列化选项");
        }

        try
        {
            var filePath = GetConfigFilePath(configType);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var config = JsonSerializer.Deserialize<TConfig>(json, options);
                return config ?? new TConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载配置文件失败: {ex.Message}");
        }

        return new TConfig();
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    private static string GetConfigFilePath(Type configType)
    {
        var fileName = _configFileNames.TryGetValue(configType, out var name) ? name : configType.Name;
        
        // 获取应用程序根目录下的Config文件夹
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var configDir = Path.Combine(appDirectory, "Config");

        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        return Path.Combine(configDir, $"{fileName}.config");
    }
}