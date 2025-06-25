using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

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
/// 配置变更队列项
/// </summary>
internal class ConfigChangeQueueItem
{
    public string FilePath { get; set; } = string.Empty;
    public Type ConfigType { get; set; } = typeof(object);
    public DateTime QueueTime { get; set; } = DateTime.Now;
    public int RetryCount { get; set; } = 0;
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
    private static readonly ConcurrentDictionary<string, DateTime> _lastReloadTimes = new();
    private static readonly ConcurrentDictionary<string, DateTime> _lastSaveTimes = new();
    private static readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan _saveIgnoreInterval = TimeSpan.FromMilliseconds(1000);
    private static readonly HashSet<string> _processingFiles = new();
    private static readonly object _processingFilesLock = new(); // 添加线程安全保护
    
    // 配置变更处理 Channel
    private static readonly Channel<ConfigChangeQueueItem> _changeChannel;
    private static readonly ChannelWriter<ConfigChangeQueueItem> _channelWriter;
    private static readonly ChannelReader<ConfigChangeQueueItem> _channelReader;
    private static readonly CancellationTokenSource _cancellationTokenSource = new();
    private static Task? _queueProcessorTask;
    
    // 队列处理配置
    private static readonly TimeSpan _duplicateFilterWindow = TimeSpan.FromMilliseconds(100); // 重复过滤窗口
    private static readonly int _maxRetryCount = 3; // 最大重试次数
    private static readonly TimeSpan _batchProcessDelay = TimeSpan.FromMilliseconds(50); // 批量处理延迟
    
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
        // 初始化Channel
        var channel = Channel.CreateUnbounded<ConfigChangeQueueItem>();
        _changeChannel = channel;
        _channelWriter = channel.Writer;
        _channelReader = channel.Reader;
        
        // 默认启用配置变更日志记录
        EnableDefaultChangeLogging();
        
        // 启动队列处理器
        _queueProcessorTask = Task.Run(ProcessChangeQueueAsync);
        XTrace.WriteLine("[INFO] 配置变更Channel处理器已启动");
    }

    /// <summary>
    /// 默认配置变更日志记录器（始终启用）
    /// </summary>
    /// <param name="logLevel">日志级别（默认为Info）</param>
    /// <param name="includeValues">是否包含具体的变更值（默认为true）</param>
    /// <param name="onlyLogChanges">是否只记录有变更的配置（默认为true）</param>
    private static void EnableDefaultChangeLogging(
        string logLevel = "Info", 
        bool includeValues = true, 
        bool onlyLogChanges = true)
    {
        // 直接订阅ConfigChangeDetails事件
        ConfigChangeDetails += (sender, details) =>
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
        };

        XTrace.WriteLine("[INFO] 配置系统默认变更日志记录已启用");
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
        
        // 始终初始化文件监控器（自动重新加载始终启用）
        InitializeFileWatcher();
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
    /// 配置文件变更事件处理 - 使用队列机制
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
            
            XTrace.WriteLine($"检测到外部配置文件变更，加入处理队列: {args.FullPath}");
            
            // 将配置变更加入处理队列
            var queueItem = new ConfigChangeQueueItem
            {
                FilePath = args.FullPath,
                ConfigType = configType,
                QueueTime = now,
                RetryCount = 0
            };
            
            _channelWriter.TryWrite(queueItem);
            XTrace.WriteLine($"[队列] 配置变更已加入队列: {configType.Name}，当前队列长度: {_changeChannel.Reader.Count}");
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
    }
    
    /// <summary>
    /// Channel队列处理器 - 使用异步迭代器处理配置变更
    /// </summary>
    private static async Task ProcessChangeQueueAsync()
    {
        var processedItems = new List<ConfigChangeQueueItem>();
        
        try
        {
            // 使用异步迭代器处理Channel中的所有项目
            await foreach (var item in _channelReader.ReadAllAsync(_cancellationTokenSource.Token).ConfigureAwait(false))
            {
                try
                {
                    var currentTime = DateTime.Now;
                    
                    // 检查是否需要过滤重复项（在很短时间内的相同文件变更）
                    var shouldSkipDuplicate = processedItems.Any(p => 
                        p.FilePath == item.FilePath && 
                        currentTime - p.QueueTime < _duplicateFilterWindow);

                    if (shouldSkipDuplicate)
                    {
                        XTrace.WriteLine($"[Channel] 过滤重复变更: {item.FilePath}");
                        continue;
                    }

                    // 处理配置变更
                    var success = await ProcessSingleConfigChangeAsync(item).ConfigureAwait(false);
                    
                    if (success)
                    {
                        XTrace.WriteLine($"[Channel] 成功处理配置变更: {item.ConfigType.Name}");
                        processedItems.Add(item);
                    }
                    else
                    {
                        // 处理失败，检查是否需要重试
                        item.RetryCount++;
                        if (item.RetryCount <= _maxRetryCount)
                        {
                            // 重新入队等待重试
                            await _channelWriter.WriteAsync(item, _cancellationTokenSource.Token).ConfigureAwait(false);
                            XTrace.WriteLine($"[Channel] 配置变更处理失败，重试第 {item.RetryCount} 次: {item.ConfigType.Name}");
                        }
                        else
                        {
                            XTrace.WriteLine($"[Channel] 配置变更处理失败，已达最大重试次数，放弃处理: {item.ConfigType.Name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
                    XTrace.WriteLine($"[Channel] 处理配置变更时发生异常: {item.ConfigType.Name}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            XTrace.WriteLine("[Channel] 配置变更处理器已取消");
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
        
        XTrace.WriteLine($"[Channel] 配置变更处理器已停止，总共处理了 {processedItems.Count} 个配置变更");
    }

    /// <summary>
    /// 异步处理单个配置变更
    /// </summary>
    /// <param name="item">队列项</param>
    /// <returns>是否处理成功</returns>
    private static async Task<bool> ProcessSingleConfigChangeAsync(ConfigChangeQueueItem item)
    {
        try
        {
            // 检查文件是否仍然存在
            if (!File.Exists(item.FilePath))
            {
                XTrace.WriteLine($"[Channel] 配置文件不存在，跳过处理: {item.FilePath}");
                return true; // 文件不存在认为是成功的（可能被删除了）
            }

            // 短暂延迟，确保文件写入完成
            await Task.Delay(_batchProcessDelay, _cancellationTokenSource.Token).ConfigureAwait(false);

            // 获取旧配置（用于事件参数）
            var oldConfig = _configs.TryGetValue(item.ConfigType, out var cached) ? cached : null;

            // 使用委托重新加载配置
            if (_configReloadDelegates.TryGetValue(item.ConfigType, out var reloadDelegate))
            {
                var newConfig = reloadDelegate();

                if (newConfig != null)
                {
                    XTrace.WriteLine($"[Channel] 配置 {item.ConfigType.Name} 重新加载成功");

                    // 比较配置差异
                    if (oldConfig != null)
                    {
                        var changeDetails = CompareConfigurations(oldConfig, newConfig);

                        // 触发配置变更详情事件
                        try
                        {
                            ConfigChangeDetails?.Invoke(null, changeDetails);
                        }
                        catch (Exception eventEx)
                        {
                            XTrace.WriteException(eventEx);
                        }
                    }

                    // 触发类型化配置变更事件
                    try
                    {
                        ConfigChanged?.Invoke(item.ConfigType, newConfig);
                    }
                    catch (Exception eventEx)
                    {
                        XTrace.WriteException(eventEx);
                    }

                    // 触发通用配置变更事件
                    try
                    {
                        var eventArgs = new ConfigChangedEventArgs(item.ConfigType, oldConfig ?? newConfig, newConfig);
                        AnyConfigChanged?.Invoke(null, eventArgs);
                    }
                    catch (Exception eventEx)
                    {
                        XTrace.WriteException(eventEx);
                    }

                    return true;
                }
                else
                {
                    XTrace.WriteLine($"[Channel] 配置 {item.ConfigType.Name} 重新加载返回null");
                    return false;
                }
            }
            else
            {
                XTrace.WriteLine($"[Channel] 未找到配置类型 {item.ConfigType.Name} 的重载委托");
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            // 取消操作不视为错误
            return false;
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            return false;
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