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
/// 配置管理器（优化版本）
/// </summary>
public static class ConfigManager
{
    // 核心数据存储
    private static readonly ConcurrentDictionary<Type, object> _configs = new();
    private static readonly ConcurrentDictionary<Type, JsonSerializerOptions> _serializerOptions = new();
    private static readonly ConcurrentDictionary<Type, string> _configFileNames = new();
    private static readonly ConcurrentDictionary<string, Type> _filePathToConfigType = new();
    private static readonly ConcurrentDictionary<Type, ConfigReloadDelegate> _configReloadDelegates = new();
    
    // 文件监控
    private static FileWatcher? _fileWatcher;
    private static readonly object _watcherLock = new();
    
    // 防抖和过滤机制
    private static readonly ConcurrentDictionary<string, DateTime> _lastSaveTimes = new();
    private static readonly TimeSpan _saveIgnoreInterval = TimeSpan.FromMilliseconds(1000);
    
    // Channel 配置变更处理
    private static readonly Channel<ConfigChangeQueueItem> _changeChannel;
    private static readonly ChannelWriter<ConfigChangeQueueItem> _channelWriter;
    private static readonly ChannelReader<ConfigChangeQueueItem> _channelReader;
    private static readonly CancellationTokenSource _cancellationTokenSource = new();
    private static readonly Task _queueProcessorTask;
    
    // 处理配置
    private static readonly TimeSpan _duplicateFilterWindow = TimeSpan.FromMilliseconds(100);
    private static readonly int _maxRetryCount = 3;
    private static readonly TimeSpan _batchProcessDelay = TimeSpan.FromMilliseconds(50);
    
    // 事件定义
    public static event Action<Type, object>? ConfigChanged;
    public static event EventHandler<ConfigChangedEventArgs>? AnyConfigChanged;
    public static event EventHandler<ConfigChangeDetails>? ConfigChangeDetails;

    // 静态构造函数 - 简化初始化
    static ConfigManager()
    {
        // 初始化 Channel（无界队列，确保不会阻塞）
        var channel = Channel.CreateUnbounded<ConfigChangeQueueItem>();
        _changeChannel = channel;
        _channelWriter = channel.Writer;
        _channelReader = channel.Reader;
        
        // 启动队列处理器
        _queueProcessorTask = Task.Run(ProcessChangeQueueAsync);
        
        // 启用默认日志记录
        EnableDefaultChangeLogging();
        
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
    /// 配置文件变更事件处理 - 简化版本
    /// </summary>
    private static void OnConfigFileChanged(object sender, FileWatcherEventArgs args)
    {
        // 快速过滤：只处理 .config 文件的修改事件
        if (!args.FullPath.EndsWith(".config", StringComparison.OrdinalIgnoreCase) ||
            args.ChangeTypes != WatcherChangeTypes.Changed ||
            !_filePathToConfigType.TryGetValue(args.FullPath, out var configType))
        {
            return;
        }

        // 检查是否是代码保存导致的变更（防抖机制）
        if (IsCodeSaveTriggered(args.FullPath))
        {
            XTrace.WriteLine($"忽略代码保存导致的文件变更: {args.FullPath}");
            return;
        }

        // 创建并入队配置变更项
        var queueItem = new ConfigChangeQueueItem
        {
            FilePath = args.FullPath,
            ConfigType = configType,
            QueueTime = DateTime.Now,
            RetryCount = 0
        };

        if (_channelWriter.TryWrite(queueItem))
        {
            XTrace.WriteLine($"[队列] 配置变更已加入队列: {configType.Name}");
        }
        else
        {
            XTrace.WriteLine($"[队列] 配置变更入队失败: {configType.Name}");
        }
    }

    /// <summary>
    /// 检查是否为代码保存触发的变更
    /// </summary>
    private static bool IsCodeSaveTriggered(string filePath)
    {
        if (_lastSaveTimes.TryGetValue(filePath, out var lastSaveTime))
        {
            return DateTime.Now - lastSaveTime < _saveIgnoreInterval;
        }
        return false;
    }

    /// <summary>
    /// Channel队列处理器 - 优化版本
    /// </summary>
    private static async Task ProcessChangeQueueAsync()
    {
        var recentlyProcessed = new ConcurrentDictionary<string, DateTime>();
        var processedCount = 0;

        try
        {
            await foreach (var item in _channelReader.ReadAllAsync(_cancellationTokenSource.Token).ConfigureAwait(false))
            {
                try
                {
                    // 重复过滤：避免短时间内处理相同文件
                    if (IsDuplicateItem(item, recentlyProcessed))
                    {
                        XTrace.WriteLine($"[Channel] 过滤重复变更: {item.ConfigType.Name}");
                        continue;
                    }

                    // 处理配置变更
                    if (await ProcessSingleConfigChangeAsync(item).ConfigureAwait(false))
                    {
                        processedCount++;
                        recentlyProcessed[item.FilePath] = DateTime.Now;
                        XTrace.WriteLine($"[Channel] 成功处理配置变更: {item.ConfigType.Name}");
                    }
                    else
                    {
                        // 重试机制
                        await HandleRetryLogic(item).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
                    XTrace.WriteLine($"[Channel] 处理配置变更异常: {item.ConfigType.Name}");
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

        XTrace.WriteLine($"[Channel] 配置变更处理器已停止，总共处理了 {processedCount} 个配置变更");
    }

    /// <summary>
    /// 检查是否为重复项
    /// </summary>
    private static bool IsDuplicateItem(ConfigChangeQueueItem item, ConcurrentDictionary<string, DateTime> recentlyProcessed)
    {
        if (recentlyProcessed.TryGetValue(item.FilePath, out var lastProcessTime))
        {
            return DateTime.Now - lastProcessTime < _duplicateFilterWindow;
        }
        return false;
    }

    /// <summary>
    /// 处理重试逻辑
    /// </summary>
    private static async Task HandleRetryLogic(ConfigChangeQueueItem item)
    {
        item.RetryCount++;
        if (item.RetryCount <= _maxRetryCount)
        {
            await _channelWriter.WriteAsync(item, _cancellationTokenSource.Token).ConfigureAwait(false);
            XTrace.WriteLine($"[Channel] 配置变更处理失败，重试第 {item.RetryCount} 次: {item.ConfigType.Name}");
        }
        else
        {
            XTrace.WriteLine($"[Channel] 配置变更处理失败，已达最大重试次数: {item.ConfigType.Name}");
        }
    }

    /// <summary>
    /// 处理单个配置变更 - 优化版本
    /// </summary>
    private static async Task<bool> ProcessSingleConfigChangeAsync(ConfigChangeQueueItem item)
    {
        try
        {
            // 文件存在性检查
            if (!File.Exists(item.FilePath))
            {
                XTrace.WriteLine($"[Channel] 配置文件不存在: {item.FilePath}");
                return true; // 文件不存在视为成功处理
            }

            // 等待文件写入完成
            await Task.Delay(_batchProcessDelay, _cancellationTokenSource.Token).ConfigureAwait(false);

            // 重新加载配置并触发事件
            return ReloadAndTriggerEvents(item);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            return false;
        }
    }

    /// <summary>
    /// 重新加载配置并触发事件
    /// </summary>
    private static bool ReloadAndTriggerEvents(ConfigChangeQueueItem item)
    {
        // 获取旧配置
        var oldConfig = _configs.TryGetValue(item.ConfigType, out var cached) ? cached : null;

        // 重新加载配置
        if (!_configReloadDelegates.TryGetValue(item.ConfigType, out var reloadDelegate))
        {
            XTrace.WriteLine($"[Channel] 未找到配置重载委托: {item.ConfigType.Name}");
            return false;
        }

        var newConfig = reloadDelegate();
        if (newConfig == null)
        {
            XTrace.WriteLine($"[Channel] 配置重新加载返回null: {item.ConfigType.Name}");
            return false;
        }

        XTrace.WriteLine($"[Channel] 配置重新加载成功: {item.ConfigType.Name}");

        // 异步触发事件（避免阻塞处理队列）
        _ = Task.Run(() => TriggerConfigChangedEvents(item.ConfigType, oldConfig, newConfig));

        return true;
    }

    /// <summary>
    /// 触发配置变更事件
    /// </summary>
    private static void TriggerConfigChangedEvents(Type configType, object? oldConfig, object newConfig)
    {
        try
        {
            // 触发详情事件
            if (oldConfig != null)
            {
                var changeDetails = CompareConfigurations(oldConfig, newConfig);
                ConfigChangeDetails?.Invoke(null, changeDetails);
            }

            // 触发类型化事件
            ConfigChanged?.Invoke(configType, newConfig);

            // 触发通用事件
            var eventArgs = new ConfigChangedEventArgs(configType, oldConfig ?? newConfig, newConfig);
            AnyConfigChanged?.Invoke(null, eventArgs);
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

    /// <summary>
    /// 清理资源（用于应用程序关闭时）
    /// </summary>
    public static void Cleanup()
    {
        try
        {
            XTrace.WriteLine("[INFO] 开始清理配置系统资源...");

            // 取消队列处理
            _cancellationTokenSource.Cancel();

            // 等待队列处理器完成（最多等待5秒）
            if (_queueProcessorTask != null && !_queueProcessorTask.IsCompleted)
            {
                _queueProcessorTask.Wait(TimeSpan.FromSeconds(5));
            }

            // 停止文件监控器
            lock (_watcherLock)
            {
                _fileWatcher?.Stop();
                // FileWatcher没有Dispose方法，只需要Stop即可
                _fileWatcher = null;
            }

            // 关闭 Channel
            _channelWriter.Complete();

            XTrace.WriteLine("[INFO] 配置系统资源清理完成");
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
        finally
        {
            _cancellationTokenSource.Dispose();
        }
    }

    /// <summary>
    /// 获取配置系统状态信息（用于监控和调试）
    /// </summary>
    public static ConfigSystemStatus GetStatus()
    {
        return new ConfigSystemStatus
        {
            RegisteredConfigCount = _configs.Count,
            FileWatcherActive = _fileWatcher != null,
            QueueProcessorRunning = _queueProcessorTask?.Status == TaskStatus.Running,
            PendingQueueItems = _changeChannel.Reader.Count,
            LastSaveOperations = _lastSaveTimes.Count
        };
    }
}

/// <summary>
/// 配置系统状态信息
/// </summary>
public class ConfigSystemStatus
{
    public int RegisteredConfigCount { get; set; }
    public bool FileWatcherActive { get; set; }
    public bool QueueProcessorRunning { get; set; }
    public int PendingQueueItems { get; set; }
    public int LastSaveOperations { get; set; }

    public override string ToString()
    {
        return $"配置系统状态: 已注册配置={RegisteredConfigCount}, 文件监控={FileWatcherActive}, " +
               $"队列处理器运行={QueueProcessorRunning}, 待处理项={PendingQueueItems}, " +
               $"最近保存操作={LastSaveOperations}";
    }
}