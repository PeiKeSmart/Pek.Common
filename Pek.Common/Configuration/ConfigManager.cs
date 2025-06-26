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
/// 配置变更事件参数（增强版本）
/// </summary>
public class ConfigChangedEventArgs : EventArgs
{
    public Type ConfigType { get; }
    public object OldConfig { get; }
    public object NewConfig { get; }
    public string ConfigName { get; }
    public List<ConfigPropertyChange> PropertyChanges { get; }
    
    public ConfigChangedEventArgs(Type configType, object oldConfig, object newConfig, List<ConfigPropertyChange> propertyChanges)
    {
        ConfigType = configType;
        OldConfig = oldConfig;
        NewConfig = newConfig;
        ConfigName = configType.Name;
        PropertyChanges = propertyChanges;
    }
    
    /// <summary>
    /// 检查指定属性是否发生变更
    /// </summary>
    public bool HasPropertyChanged(string propertyName)
    {
        return PropertyChanges.Any(c => c.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取指定属性的变更信息
    /// </summary>
    public ConfigPropertyChange? GetPropertyChange(string propertyName)
    {
        return PropertyChanges.FirstOrDefault(c => c.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 是否有任何属性变更
    /// </summary>
    public bool HasChanges => PropertyChanges.Count > 0;
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
/// 配置管理器（极简版本）
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
    
    // 简化的防抖机制
    private static readonly ConcurrentDictionary<string, DateTime> _lastSaveTimes = new();
    private static readonly TimeSpan _saveIgnoreInterval = TimeSpan.FromMilliseconds(1000);
    
    // Channel 配置变更处理
    private static readonly ChannelWriter<ConfigChangeQueueItem> _channelWriter;
    private static readonly ChannelReader<ConfigChangeQueueItem> _channelReader;
    private static readonly CancellationTokenSource _cancellationTokenSource = new();
    private static readonly Task _queueProcessorTask;
    
    // 自动清理机制 - Channel版本析构函数清理
    private static readonly ChannelCleanupHelper _cleanupHelper = new();
    
    // 唯一事件
    public static event EventHandler<ConfigChangedEventArgs>? ConfigChanged;

    // 静态构造函数
    static ConfigManager()
    {
        // 初始化 Channel
        var channel = Channel.CreateUnbounded<ConfigChangeQueueItem>();
        _channelWriter = channel.Writer;
        _channelReader = channel.Reader;
        
        // 启动队列处理器
        _queueProcessorTask = Task.Run(ProcessChangeQueueAsync);
        
        // 默认日志记录（简化版本）
        ConfigChanged += (sender, e) =>
        {
            // 记录变更日志
            if (e.HasChanges)
            {
                var changes = string.Join(", ", e.PropertyChanges.Take(3).Select(c => c.ToString()));
                var moreInfo = e.PropertyChanges.Count > 3 ? $" 等{e.PropertyChanges.Count}个属性" : "";
                XTrace.WriteLine($"[INFO] 配置 {e.ConfigName} 变更: {changes}{moreInfo}");
            }
            
            // ConfigManager 已经在 ReloadConfigInternal 中更新了 _configs 缓存
            // Config<T>.Current 会自动从 ConfigManager.GetConfig<T>() 获取最新实例
            // 无需额外的实例同步操作
        };
        
        XTrace.WriteLine("[INFO] 配置系统已启动（Channel + 析构函数清理）");
    }

    /// <summary>
    /// Channel自动清理辅助类 - 通过析构函数实现自动资源释放
    /// </summary>
    private sealed class ChannelCleanupHelper
    {
        private volatile bool _isDisposed = false;

        /// <summary>
        /// 析构函数 - 在垃圾回收时自动清理Channel相关资源
        /// </summary>
        ~ChannelCleanupHelper()
        {
            if (!_isDisposed)
            {
                PerformCleanup();
            }
        }

        /// <summary>
        /// 手动清理资源
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                PerformCleanup();
                _isDisposed = true;
                GC.SuppressFinalize(this); // 抑制析构函数调用
            }
        }

        /// <summary>
        /// 执行实际的清理操作
        /// </summary>
        private void PerformCleanup()
        {
            try
            {
                XTrace.WriteLine("[INFO] 开始自动清理Channel配置系统资源...");

                // 1. 停止Channel写入
                try
                {
                    _channelWriter?.Complete();
                    XTrace.WriteLine("[INFO] Channel写入已停止");
                }
                catch (Exception ex)
                {
                    XTrace.WriteLine($"[WARNING] 停止Channel写入时出错: {ex.Message}");
                }
                
                // 2. 取消后台任务
                try
                {
                    _cancellationTokenSource?.Cancel();
                    XTrace.WriteLine("[INFO] 后台任务取消信号已发送");
                }
                catch (Exception ex)
                {
                    XTrace.WriteLine($"[WARNING] 取消后台任务时出错: {ex.Message}");
                }
                
                // 3. 等待后台任务完成（有超时限制，避免析构函数阻塞）
                if (_queueProcessorTask != null && !_queueProcessorTask.IsCompleted)
                {
                    // 在析构函数中使用较短的超时时间
                    if (_queueProcessorTask.Wait(TimeSpan.FromSeconds(3)))
                    {
                        XTrace.WriteLine("[INFO] 后台任务已正常完成");
                    }
                    else
                    {
                        XTrace.WriteLine("[WARNING] 后台任务未能在3秒内完成，强制继续清理");
                    }
                }
                
                // 4. 清理文件监控器
                lock (_watcherLock)
                {
                    if (_fileWatcher != null)
                    {
                        try
                        {
                            _fileWatcher.Stop();
                            _fileWatcher = null;
                            XTrace.WriteLine("[INFO] 文件监控器已自动停止");
                        }
                        catch (Exception ex)
                        {
                            XTrace.WriteLine($"[WARNING] 停止文件监控器时出错: {ex.Message}");
                        }
                    }
                }
                
                // 5. 释放CancellationTokenSource
                try
                {
                    _cancellationTokenSource?.Dispose();
                    XTrace.WriteLine("[INFO] CancellationTokenSource已释放");
                }
                catch (Exception ex)
                {
                    XTrace.WriteLine($"[WARNING] 释放CancellationTokenSource时出错: {ex.Message}");
                }

                XTrace.WriteLine("[INFO] Channel配置系统资源自动清理完成");
            }
            catch (Exception ex)
            {
                // 清理过程中的异常不应该抛出，静默处理
                try
                {
                    XTrace.WriteLine($"[ERROR] 自动清理过程中发生异常: {ex.Message}");
                }
                catch
                {
                    // 如果连日志都无法记录，则完全静默
                }
            }
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
    /// 获取配置实例（通过Type）
    /// </summary>
    /// <param name="configType">配置类型</param>
    /// <param name="forceReload">是否强制重新加载</param>
    /// <returns>配置实例</returns>
    public static object GetConfig(Type configType, bool forceReload = false)
    {
        // 性能优化：先检查缓存，避免不必要的锁争用
        if (!forceReload && _configs.TryGetValue(configType, out var cachedConfig))
        {
            return cachedConfig;
        }

        // 双重检查锁模式，避免重复加载
        lock (_configs)
        {
            if (!forceReload && _configs.TryGetValue(configType, out cachedConfig))
            {
                return cachedConfig;
            }

            // 使用重载委托加载配置
            if (_configReloadDelegates.TryGetValue(configType, out var reloadDelegate))
            {
                var config = reloadDelegate();
                _configs[configType] = config;
                return config;
            }
            else
            {
                throw new InvalidOperationException($"配置类型 {configType.Name} 未注册");
            }
        }
    }

    /// <summary>
    /// 尝试获取序列化选项
    /// </summary>
    /// <param name="configType">配置类型</param>
    /// <param name="options">序列化选项</param>
    /// <returns>是否获取成功</returns>
    public static bool TryGetSerializerOptions(Type configType, out JsonSerializerOptions options)
    {
        return _serializerOptions.TryGetValue(configType, out options!);
    }

    /// <summary>
    /// 加载配置（简化版本）
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
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    XTrace.WriteLine($"配置文件为空，使用默认配置: {filePath}");
                    return new TConfig();
                }

                try
                {
                    var config = JsonSerializer.Deserialize<TConfig>(json, options);
                    return config ?? new TConfig();
                }
                catch (JsonException jsonEx)
                {
                    XTrace.WriteLine($"配置文件JSON格式错误: {filePath}, 错误: {jsonEx.Message}");
                    return new TConfig(); // 直接返回默认配置，不进行备份
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
    /// Channel队列处理器 - 简化版本
    /// </summary>
    private static async Task ProcessChangeQueueAsync()
    {
        var processedCount = 0;

        try
        {
            await foreach (var item in _channelReader.ReadAllAsync(_cancellationTokenSource.Token).ConfigureAwait(false))
            {
                try
                {
                    // 简化处理：直接处理配置变更，不做复杂的重复过滤
                    if (await ProcessSingleConfigChangeAsync(item).ConfigureAwait(false))
                    {
                        processedCount++;
                        XTrace.WriteLine($"[Channel] 成功处理配置变更: {item.ConfigType.Name}");
                    }
                    else
                    {
                        XTrace.WriteLine($"[Channel] 配置变更处理失败: {item.ConfigType.Name}");
                    }
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
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
    /// 处理单个配置变更 - 简化版本
    /// </summary>
    private static async Task<bool> ProcessSingleConfigChangeAsync(ConfigChangeQueueItem item)
    {
        try
        {
            // 文件存在性检查
            if (!File.Exists(item.FilePath))
            {
                return true; // 文件不存在视为成功处理
            }

            // 简单延迟，确保文件写入完成
            await Task.Delay(200, _cancellationTokenSource.Token).ConfigureAwait(false);

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

        // 触发配置变更事件
        ConfigChanged?.Invoke(null, new ConfigChangedEventArgs(item.ConfigType, oldConfig ?? newConfig, newConfig, GetPropertyChanges(oldConfig, newConfig)));

        return true;
    }

    /// <summary>
    /// 比较两个配置对象并获取属性变更信息（使用独立的JSON对比工具）
    /// </summary>
    private static List<ConfigPropertyChange> GetPropertyChanges(object? oldConfig, object newConfig)
    {
        var configType = newConfig.GetType();
        
        try
        {
            // 使用独立的JSON对比工具类
            if (_serializerOptions.TryGetValue(configType, out var options))
            {
                return ConfigJsonComparer.GetPropertyChanges(oldConfig, newConfig, configType, options);
            }
            else
            {
                // 回退到简单比较
                return ConfigJsonComparer.GetPropertyChangesSimple(oldConfig, newConfig);
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            return ConfigJsonComparer.GetPropertyChangesSimple(oldConfig, newConfig);
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