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
    private static TConfig? _current;
    private static readonly object _lock = new object();

    /// <summary>
    /// 当前配置实例
    /// </summary>
    public static TConfig Current
    {
        get
        {
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
    /// 手动注册配置类型（AOT兼容）
    /// </summary>
    /// <param name="typeInfoResolver">类型信息解析器</param>
    /// <param name="fileName">配置文件名（可选）</param>
    /// <param name="writeIndented">是否格式化JSON（可选）</param>
    /// <param name="useCamelCase">是否使用驼峰命名（可选）</param>
    public static void RegisterForAot(
        JsonSerializerContext typeInfoResolver,
        string? fileName = null,
        bool writeIndented = true,
        bool useCamelCase = true)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = typeInfoResolver,
            WriteIndented = writeIndented
        };

        if (useCamelCase)
        {
            jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        ConfigManager.RegisterConfig<TConfig>(jsonOptions, fileName);
    }
}