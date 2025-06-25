using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// 当前配置实例
    /// </summary>
    public static TConfig Current
    {
        get
        {
            // 确保配置类已初始化
            EnsureInitialized();
            
            if (_current == null)
            {
                lock (_lock)
                {
                    _current ??= ConfigManager.GetConfig<TConfig>();
                }
            }
            return _current;
        }
    }
    
    /// <summary>
    /// 确保配置类已初始化
    /// </summary>
    private static void EnsureInitialized()
    {
        if (!_initialized)
        {
            lock (_initLock)
            {
                if (!_initialized)
                {
                    // 执行初始化逻辑，触发静态构造函数
                    RuntimeHelpers.RunClassConstructor(typeof(TConfig).TypeHandle);
                    _initialized = true;
                }
            }
        }
    }

    /// <summary>
    /// 重新加载配置
    /// </summary>
    public static void Reload()
    {
        lock (_lock)
        {
            _current = ConfigManager.GetConfig<TConfig>(forceReload: true);
        }
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