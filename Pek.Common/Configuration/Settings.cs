using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pek.Configuration;

/// <summary>
/// 配置
/// </summary>
public class Settings : Config<Settings>
{
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string? Version { get; set; } = "1.0.0";

    /// <summary>
    /// 是否启用调试模式
    /// </summary>
    public bool Debug { get; set; } = false;

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 静态构造函数，注册配置
    /// </summary>
    static Settings()
    {
        ConfigManager.RegisterConfig<Settings>(GetJsonSerializerOptions(), "Settings");
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            TypeInfoResolver = SettingsJsonContext.Default,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}

/// <summary>
/// AOT兼容的JSON序列化上下文
/// </summary>
[JsonSerializable(typeof(Settings))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
public partial class SettingsJsonContext : JsonSerializerContext
{
}