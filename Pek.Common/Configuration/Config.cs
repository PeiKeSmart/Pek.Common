using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NewLife.Log;

namespace Pek.Configuration;

/// <summary>配置文件基类</summary>
/// <remarks>
/// 标准用法：通过ConfigManager.GetConfig&lt;TConfig&gt;()获取配置实例
/// 每个配置类需要注册对应的JsonSerializerContext
/// </remarks>
public abstract class Config
{
    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public virtual void Save()
    {
        ConfigManager.SaveConfig(this);
    }
}

/// <summary>
/// 泛型配置基类
/// </summary>
/// <typeparam name="TConfig"></typeparam>
public abstract class Config<TConfig> : Config where TConfig : Config<TConfig>, new()
{
    // 标记配置类是否已初始化
    private static bool _initialized = false;
    private static readonly object _initLock = new object();
    
    private static TConfig? _current;
    private static readonly object _lock = new object();
    
    // 静态构造函数，用于订阅配置变更事件
    static Config()
    {
        // 订阅配置变更事件
        ConfigManager.ConfigChanged += OnConfigChanged;
    }
    
    /// <summary>
    /// 配置变更事件处理（增强异常保护）
    /// </summary>
    /// <param name="configType">配置类型</param>
    /// <param name="newConfig">新的配置实例</param>
    private static void OnConfigChanged(Type configType, object newConfig)
    {
        try
        {
            if (configType == typeof(TConfig) && newConfig is TConfig typedConfig)
            {
                lock (_lock)
                {
                    _current = typedConfig;
                }
                
                // 触发配置变更通知事件（带异常保护）
                try
                {
                    ConfigurationChanged?.Invoke(typedConfig);
                }
                catch (Exception ex)
                {
                    // 记录异常但不影响配置更新流程
                    XTrace.WriteException(ex);
                }
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
    }

    /// <summary>
    /// 配置变更通知事件
    /// </summary>
    /// <remarks>
    /// 当配置文件被外部修改并自动重新加载后，此事件会被触发。
    /// 您可以订阅此事件来获得配置变更的通知。
    /// </remarks>
    public static event Action<TConfig>? ConfigurationChanged;

    /// <summary>
    /// 当前配置实例（性能优化版本）
    /// </summary>
    public static TConfig Current
    {
        get
        {
            // 确保配置类已初始化
            EnsureInitialized();
            
            // 双重检查锁模式优化
            if (_current == null)
            {
                lock (_lock)
                {
                    if (_current == null)
                    {
                        _current = ConfigManager.GetConfig<TConfig>();
                    }
                }
            }
            return _current;
        }
    }
    
    /// <summary>
    /// 确保配置类已初始化（AOT兼容版本）
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)] // 防止内联，确保正确的初始化顺序
    private static void EnsureInitialized()
    {
        if (!_initialized)
        {
            lock (_initLock)
            {
                if (!_initialized)
                {
                    try
                    {
                        // AOT兼容的初始化方式
                        RuntimeHelpers.RunClassConstructor(typeof(TConfig).TypeHandle);
                        _initialized = true;
                    }
                    catch (Exception ex)
                    {
                        XTrace.WriteException(ex);
                        // 即使初始化失败，也要标记为已初始化，避免重复尝试
                        _initialized = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 重新加载配置（线程安全版本）
    /// </summary>
    public static void Reload()
    {
        lock (_lock)
        {
            try
            {
                _current = ConfigManager.GetConfig<TConfig>(forceReload: true);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
                throw; // 重新抛出异常，让调用者知道重新加载失败
            }
        }
    }
    
    /// <summary>
    /// 启用或禁用此配置类型的自动重新加载功能
    /// </summary>
    /// <param name="enabled">是否启用自动重新加载</param>
    /// <remarks>
    /// 默认情况下，所有配置类型都启用自动重新加载。
    /// 如果您希望某个特定的配置类型不自动重新加载，可以调用此方法禁用。
    /// </remarks>
    public static void SetAutoReload(bool enabled)
    {
        ConfigManager.SetAutoReload(enabled);
    }
    
    /// <summary>
    /// 通用配置注册方法（AOT兼容）
    /// </summary>
    /// <typeparam name="TJsonContext">JSON序列化上下文类型</typeparam>
    /// <param name="jsonContext">JSON序列化上下文实例</param>
    /// <param name="fileName">配置文件名（可选）</param>
    /// <param name="writeIndented">是否格式化JSON（可选）</param>
    /// <param name="useCamelCase">是否使用驼峰命名（可选）</param>
    public static void RegisterConfigForAot<TJsonContext>(
        TJsonContext jsonContext,
        string? fileName = null,
        bool writeIndented = true,
        bool useCamelCase = true) where TJsonContext : JsonSerializerContext
    {
        var jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = jsonContext,
            WriteIndented = writeIndented,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (useCamelCase)
        {
            jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        ConfigManager.RegisterConfig<TConfig>(jsonOptions, fileName);
    }
}