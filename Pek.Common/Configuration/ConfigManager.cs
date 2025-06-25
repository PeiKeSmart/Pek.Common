using System.Collections.Concurrent;
using System.Text.Json;

using NewLife.Log;
using Pek.IO;

namespace Pek.Configuration;

/// <summary>
/// 配置重新加载委托
/// </summary>
/// <returns>重新加载的配置对象</returns>
public delegate object ConfigReloadDelegate();

/// <summary>
/// 配置变更事件参数
/// </summary>
public class ConfigChangedEventArgs : EventArgs
{
    public Type ConfigType { get; }
    public object OldConfig { get; }
    public object NewConfig { get; }
    public string ConfigName { get; }
    
    public ConfigChangedEventArgs(Type configType, object oldConfig, object newConfig)
    {
        ConfigType = configType;
        OldConfig = oldConfig;
        NewConfig = newConfig;
        ConfigName = configType.Name;
    }
}

/// <summary>
/// 配置属性变更详情
/// </summary>
public class ConfigPropertyChange
{
    public string PropertyName { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public Type PropertyType { get; set; } = typeof(object);
    
    public override string ToString()
    {
        return $"{PropertyName}: {OldValue} → {NewValue}";
    }
}

/// <summary>
/// 配置变更详情
/// </summary>
public class ConfigChangeDetails
{
    public string ConfigName { get; set; } = string.Empty;
    public Type ConfigType { get; set; } = typeof(object);
    public DateTime ChangeTime { get; set; } = DateTime.Now;
    public List<ConfigPropertyChange> PropertyChanges { get; set; } = new();
    
    public bool HasChanges => PropertyChanges.Count > 0;
    
    public override string ToString()
    {
        if (!HasChanges)
            return $"配置 {ConfigName} 无变更";
            
        var changes = string.Join(", ", PropertyChanges.Select(c => c.ToString()));
        return $"配置 {ConfigName} 变更: {changes}";
    }
}

/// <summary>
/// 配置管理器
/// </summary>
public static class ConfigManager
{
    private static readonly ConcurrentDictionary<Type, object> _configs = new();
    private static readonly ConcurrentDictionary<Type, JsonSerializerOptions> _serializerOptions = new();
    private static readonly ConcurrentDictionary<Type, string> _configFileNames = new();
    private static readonly ConcurrentDictionary<string, Type> _filePathToConfigType = new();
    
    // 配置重载委托缓存（消除反射依赖）
    private static readonly ConcurrentDictionary<Type, ConfigReloadDelegate> _configReloadDelegates = new();
    
    private static FileWatcher? _fileWatcher;
    private static readonly object _watcherLock = new();
    private static bool _autoReloadEnabled = true;
    private static readonly ConcurrentDictionary<string, DateTime> _lastReloadTimes = new();
    private static readonly ConcurrentDictionary<string, DateTime> _lastSaveTimes = new();
    private static readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan _saveIgnoreInterval = TimeSpan.FromMilliseconds(1000);
    private static bool _defaultLoggingEnabled = false;
    private static readonly HashSet<string> _processingFiles = new();
    private static readonly object _processingFilesLock = new(); // 添加线程安全保护
    
    /// <summary>
    /// 配置文件变更事件（类型化）
    /// </summary>
    public static event Action<Type, object>? ConfigChanged;
    
    /// <summary>
    /// 通用配置变更事件（推荐使用）
    /// </summary>
    public static event EventHandler<ConfigChangedEventArgs>? AnyConfigChanged;

    /// <summary>
    /// 配置变更详情事件（包含详细的属性变更信息）
    /// </summary>
    public static event EventHandler<ConfigChangeDetails>? ConfigChangeDetails;

    // 静态构造函数，自动启用默认配置变更日志记录
    static ConfigManager()
    {
        // 默认启用配置变更日志记录
        EnableDefaultChangeLogging();
    }

    /// <summary>
    /// 订阅特定类型的配置变更事件
    /// </summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    public static void SubscribeConfigChanged<TConfig>(Action<TConfig> handler) where TConfig : Config
    {
        AnyConfigChanged += (sender, e) =>
        {
            if (e.ConfigType == typeof(TConfig) && e.NewConfig is TConfig typedConfig)
            {
                handler(typedConfig);
            }
        };
    }

    /// <summary>
    /// 订阅配置变更事件（带旧值和新值比较）
    /// </summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    public static void SubscribeConfigChanged<TConfig>(Action<TConfig, TConfig> handler) where TConfig : Config
    {
        AnyConfigChanged += (sender, e) =>
        {
            if (e.ConfigType == typeof(TConfig) && 
                e.OldConfig is TConfig oldConfig && 
                e.NewConfig is TConfig newConfig)
            {
                handler(oldConfig, newConfig);
            }
        };
    }

    /// <summary>
    /// 订阅配置变更详情事件（自动检测所有属性变更）
    /// </summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    public static void SubscribeConfigChangeDetails<TConfig>(Action<ConfigChangeDetails> handler) where TConfig : Config
    {
        ConfigChangeDetails += (sender, details) =>
        {
            if (details.ConfigType == typeof(TConfig))
            {
                handler(details);
            }
        };
    }

    /// <summary>
    /// 订阅所有配置变更事件
    /// </summary>
    /// <param name="handler">事件处理程序</param>
    public static void SubscribeAllConfigChanged(Action<ConfigChangedEventArgs> handler)
    {
        AnyConfigChanged += (sender, e) => handler(e);
    }

    /// <summary>
    /// 订阅所有配置变更详情事件
    /// </summary>
    /// <param name="handler">事件处理程序</param>
    public static void SubscribeAllConfigChangeDetails(Action<ConfigChangeDetails> handler)
    {
        ConfigChangeDetails += (sender, details) => handler(details);
    }

    /// <summary>
    /// 默认配置变更日志记录器
    /// </summary>
    /// <param name="logLevel">日志级别（默认为Info）</param>
    /// <param name="includeValues">是否包含具体的变更值（默认为true）</param>
    /// <param name="onlyLogChanges">是否只记录有变更的配置（默认为true）</param>
    public static void EnableDefaultChangeLogging(
        string logLevel = "Info", 
        bool includeValues = true, 
        bool onlyLogChanges = true)
    {
        // 避免重复订阅
        if (_defaultLoggingEnabled)
        {
            return;
        }

        SubscribeAllConfigChangeDetails(details =>
        {
            if (onlyLogChanges && !details.HasChanges)
                return;

            var message = includeValues 
                ? details.ToString() 
                : $"配置 {details.ConfigName} 已变更 ({details.PropertyChanges.Count} 个属性)";

            switch (logLevel.ToLower())
            {
                case "debug":
                    XTrace.WriteLine($"[DEBUG] {message}");
                    break;
                case "info":
                    XTrace.WriteLine($"[INFO] {message}");
                    break;
                case "warn":
                case "warning":
                    XTrace.WriteLine($"[WARN] {message}");
                    break;
                default:
                    XTrace.WriteLine(message);
                    break;
            }
        });

        _defaultLoggingEnabled = true;
        XTrace.WriteLine("[INFO] 配置系统默认变更日志记录已启用");
    }

    /// <summary>
    /// 禁用默认配置变更日志记录
    /// </summary>
    public static void DisableDefaultChangeLogging()
    {
        if (!_defaultLoggingEnabled)
        {
            return;
        }

        // 注意：这里只是标记为禁用，实际的事件处理器仍然存在
        // 如果需要完全移除，需要更复杂的事件管理机制
        _defaultLoggingEnabled = false;
        XTrace.WriteLine("[INFO] 配置系统默认变更日志记录已禁用");
    }

    /// <summary>
    /// 检查默认日志记录是否已启用
    /// </summary>
    public static bool IsDefaultChangeLoggingEnabled => _defaultLoggingEnabled;

    /// <summary>
    /// 启用或禁用配置文件自动重新加载
    /// </summary>
    /// <param name="enabled">是否启用自动重新加载</param>
    public static void SetAutoReload(bool enabled)
    {
        _autoReloadEnabled = enabled;
        if (enabled)
        {
            InitializeFileWatcher();
        }
        else
        {
            StopFileWatcher();
        }
    }
    
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
        
        // 注册配置重载委托（消除反射依赖）
        _configReloadDelegates[configType] = () => ReloadConfigInternal<TConfig>();
        
        // 建立文件路径到配置类型的映射
        var filePath = GetConfigFilePath(configType);
        _filePathToConfigType[filePath] = configType;
        
        // 如果启用了自动重新加载，初始化文件监控器
        if (_autoReloadEnabled)
        {
            InitializeFileWatcher();
        }
    }

    /// <summary>
    /// 获取配置实例（性能优化版本）
    /// </summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    /// <param name="forceReload">是否强制重新加载</param>
    /// <returns></returns>
    public static TConfig GetConfig<TConfig>(bool forceReload = false) where TConfig : Config, new()
    {
        var configType = typeof(TConfig);

        // 性能优化：先检查缓存，避免不必要的锁争用
        if (!forceReload && _configs.TryGetValue(configType, out var cachedConfig))
        {
            return (TConfig)cachedConfig;
        }

        // 双重检查锁模式，避免重复加载
        lock (_configs)
        {
            if (!forceReload && _configs.TryGetValue(configType, out cachedConfig))
            {
                return (TConfig)cachedConfig;
            }

            var config = LoadConfig<TConfig>();
            _configs[configType] = config;
            return config;
        }
    }

    /// <summary>
    /// 加载配置（增强异常处理和性能优化）
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
                // 性能优化：使用异步文件读取避免阻塞
                var json = File.ReadAllText(filePath);
                
                // 验证JSON不为空
                if (string.IsNullOrWhiteSpace(json))
                {
                    XTrace.WriteLine($"配置文件为空，使用默认配置: {filePath}");
                    return new TConfig();
                }

                try
                {
                    var config = JsonSerializer.Deserialize<TConfig>(json, options);
                    if (config == null)
                    {
                        XTrace.WriteLine($"配置反序列化失败，使用默认配置: {filePath}");
                        return new TConfig();
                    }
                    return config;
                }
                catch (JsonException jsonEx)
                {
                    XTrace.WriteLine($"配置文件JSON格式错误: {filePath}, 错误: {jsonEx.Message}");
                    // JSON格式错误时，备份损坏的文件
                    BackupCorruptedConfigFile(filePath);
                    return new TConfig();
                }
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }

        return new TConfig();
    }

    /// <summary>
    /// 备份损坏的配置文件
    /// </summary>
    /// <param name="filePath">损坏的文件路径</param>
    private static void BackupCorruptedConfigFile(string filePath)
    {
        try
        {
            var backupPath = $"{filePath}.corrupted.{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(filePath, backupPath, true);
            XTrace.WriteLine($"已备份损坏的配置文件: {backupPath}");
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
    }

    /// <summary>
    /// 保存配置（性能优化版本）
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
            
            // 记录保存时间，用于过滤掉代码保存触发的文件监控事件
            _lastSaveTimes[filePath] = DateTime.Now;

            // 性能优化：先序列化到内存，再一次性写入文件
            var json = JsonSerializer.Serialize(config, configType, options);

            // 确保目录存在
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 原子性写入：先写入临时文件，再替换原文件
            var tempFilePath = $"{filePath}.tmp";
            File.WriteAllText(tempFilePath, json);
            
            // 原子性替换文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Move(tempFilePath, filePath);

            XTrace.WriteLine($"保存配置到文件：{filePath}");

            // 更新缓存
            _configs[configType] = config;
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            throw new InvalidOperationException($"保存配置文件失败: {ex.Message}", ex);
        }
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
    
    /// <summary>
    /// 初始化文件监控器
    /// </summary>
    private static void InitializeFileWatcher()
    {
        lock (_watcherLock)
        {
            if (_fileWatcher != null)
            {
                return;
            }
            
            try
            {
                // 获取Config目录路径
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var configDir = Path.Combine(appDirectory, "Config");
                
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // 创建文件监控器
                _fileWatcher = new FileWatcher(new[] { configDir });
                _fileWatcher.EventHandler += OnConfigFileChanged;
                _fileWatcher.Start();
                
                XTrace.WriteLine("配置文件监控器已启动");
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
        }
    }
    
    /// <summary>
    /// 停止文件监控器
    /// </summary>
    private static void StopFileWatcher()
    {
        lock (_watcherLock)
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.Stop();
                _fileWatcher = null;
                XTrace.WriteLine("配置文件监控器已停止");
            }
        }
    }
    
    /// <summary>
    /// 比较两个配置对象的差异（AOT兼容统一方案）
    /// </summary>
    /// <param name="oldConfig">旧配置对象</param>
    /// <param name="newConfig">新配置对象</param>
    /// <returns>配置变更详情</returns>
    private static ConfigChangeDetails CompareConfigurations(object oldConfig, object newConfig)
    {
        var configType = newConfig.GetType();
        var details = new ConfigChangeDetails
        {
            ConfigName = configType.Name,
            ConfigType = configType,
            ChangeTime = DateTime.Now
        };

        try
        {
            // 统一使用JSON序列化比较方案（AOT友好，无反射）
            if (_serializerOptions.TryGetValue(configType, out var options))
            {
                var oldJson = JsonSerializer.Serialize(oldConfig, configType, options);
                var newJson = JsonSerializer.Serialize(newConfig, configType, options);
                
                if (oldJson != newJson)
                {
                    // JSON不同时，记录配置已变更
                    details.PropertyChanges.Add(new ConfigPropertyChange
                    {
                        PropertyName = "Configuration",
                        OldValue = "配置已变更",
                        NewValue = "新配置已生效",
                        PropertyType = configType
                    });
                    
                    // 可选：如果需要更详细的信息，可以记录JSON差异的字符数
                    var diffLength = Math.Abs(oldJson.Length - newJson.Length);
                    if (diffLength > 0)
                    {
                        details.PropertyChanges.Add(new ConfigPropertyChange
                        {
                            PropertyName = "JsonSizeDiff",
                            OldValue = oldJson.Length,
                            NewValue = newJson.Length,
                            PropertyType = typeof(int)
                        });
                    }
                }
            }
            else
            {
                // 如果没有序列化选项，回退到简单比较
                if (!ReferenceEquals(oldConfig, newConfig))
                {
                    details.PropertyChanges.Add(new ConfigPropertyChange
                    {
                        PropertyName = "Configuration",
                        OldValue = "原始配置",
                        NewValue = "已更新配置",
                        PropertyType = configType
                    });
                }
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            
            // 发生异常时的回退处理
            details.PropertyChanges.Add(new ConfigPropertyChange
            {
                PropertyName = "Configuration",
                OldValue = "比较过程出错",
                NewValue = "配置已变更",
                PropertyType = configType
            });
        }

        return details;
    }

    /// <summary>
    /// 配置文件变更事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">文件监控事件参数</param>
    private static void OnConfigFileChanged(object sender, FileWatcherEventArgs args)
    {
        try
        {
            // 只处理.config文件的变更
            if (!args.FullPath.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            
            // 只处理文件修改事件
            if (args.ChangeTypes != WatcherChangeTypes.Changed)
            {
                return;
            }
            
            // 查找对应的配置类型
            if (!_filePathToConfigType.TryGetValue(args.FullPath, out var configType))
            {
                return;
            }
            
            var now = DateTime.Now;
            var fileKey = args.FullPath;
            
            // 检查是否是代码保存导致的文件变更
            if (_lastSaveTimes.TryGetValue(fileKey, out var lastSaveTime))
            {
                if (now - lastSaveTime < _saveIgnoreInterval)
                {
                    // 在保存忽略间隔内，说明是代码保存导致的变更，忽略此次事件
                    XTrace.WriteLine($"忽略代码保存导致的文件变更: {args.FullPath}");
                    return;
                }
            }
            
            // 防抖处理：检查是否在短时间内已经处理过同一个文件
            if (_lastReloadTimes.TryGetValue(fileKey, out var lastReloadTime))
            {
                if (now - lastReloadTime < _debounceInterval)
                {
                    // 在防抖间隔内，忽略此次变更
                    return;
                }
            }
            
            // 更新最后重新加载时间
            _lastReloadTimes[fileKey] = now;
            
            XTrace.WriteLine($"检测到外部配置文件变更: {args.FullPath}");
            
            // 延迟一小段时间，避免文件正在写入时读取
            Task.Delay(100).ContinueWith(_ =>
            {
                try
                {
                    // 添加重入保护 - 防止死循环
                    if (_processingFiles.Contains(fileKey))
                    {
                        XTrace.WriteLine($"文件 {fileKey} 正在处理中，跳过此次变更");
                        return;
                    }
                    
                    lock (_processingFilesLock) // 添加锁
                    {
                        _processingFiles.Add(fileKey);
                    }
                    
                    try
                    {
                        // 获取旧配置（用于事件参数）
                        var oldConfig = _configs.TryGetValue(configType, out var cached) ? cached : null;
                        
                        // 使用委托重新加载配置（完全消除反射）
                        if (_configReloadDelegates.TryGetValue(configType, out var reloadDelegate))
                        {
                            var newConfig = reloadDelegate();
                            
                            if (newConfig != null)
                            {
                                XTrace.WriteLine($"配置 {configType.Name} 已自动重新加载");
                                
                                // 比较配置差异
                                if (oldConfig != null)
                                {
                                    var changeDetails = CompareConfigurations(oldConfig, newConfig);
                                    
                                    // 在事件触发时添加异常保护
                                    try
                                    {
                                        ConfigChangeDetails?.Invoke(null, changeDetails);
                                    }
                                    catch (Exception eventEx)
                                    {
                                        XTrace.WriteException(eventEx);
                                    }
                                }
                                
                                // 触发类型化配置变更事件（带异常保护）
                                try
                                {
                                    ConfigChanged?.Invoke(configType, newConfig);
                                }
                                catch (Exception eventEx)
                                {
                                    XTrace.WriteException(eventEx);
                                }
                                
                                // 触发通用配置变更事件（带异常保护）
                                try
                                {
                                    var eventArgs = new ConfigChangedEventArgs(configType, oldConfig ?? newConfig, newConfig);
                                    AnyConfigChanged?.Invoke(null, eventArgs);
                                }
                                catch (Exception eventEx)
                                {
                                    XTrace.WriteException(eventEx);
                                }
                            }
                        }
                        else
                        {
                            XTrace.WriteLine($"未找到配置类型 {configType.Name} 的重载委托，请确保已正确注册配置");
                        }
                    }
                    finally
                    {
                        lock (_processingFilesLock) // 添加锁
                        {
                            _processingFiles.Remove(fileKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
                }
            });
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
    }
    
    /// <summary>
    /// 内部重新加载配置方法
    /// </summary>
    /// <typeparam name="TConfig">配置类型</typeparam>
    /// <returns>重新加载的配置实例</returns>
    private static TConfig ReloadConfigInternal<TConfig>() where TConfig : Config, new()
    {
        var configType = typeof(TConfig);
        var config = LoadConfig<TConfig>();
        _configs[configType] = config;
        return config;
    }
}