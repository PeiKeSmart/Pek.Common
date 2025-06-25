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
    
    /// <summary>
    /// 当前配置实例（直接从ConfigManager获取，无需本地缓存）
    /// </summary>
    public static TConfig Current
    {
        get
        {
            // 确保配置类已初始化
            EnsureInitialized();
            
            // 直接从 ConfigManager 获取最新实例，无需本地缓存
            return ConfigManager.GetConfig<TConfig>();
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
        // 直接调用 ConfigManager 的强制重新加载
        ConfigManager.GetConfig<TConfig>(forceReload: true);
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

    /// <summary>
    /// 简化配置注册方法（适用于包含TConfig类型的JsonSerializerContext）
    /// </summary>
    /// <typeparam name="TJsonContext">包含TConfig类型的JSON序列化上下文类型</typeparam>
    /// <param name="fileName">配置文件名（可选）</param>
    /// <param name="writeIndented">是否格式化JSON（可选）</param>
    /// <param name="useCamelCase">是否使用驼峰命名（可选）</param>
    public static void RegisterForAot<TJsonContext>(
        string? fileName = null,
        bool writeIndented = true,
        bool useCamelCase = true) where TJsonContext : JsonSerializerContext, new()
    {
        var jsonContext = new TJsonContext();
        RegisterConfigForAot(jsonContext, fileName, writeIndented, useCamelCase);
    }
}