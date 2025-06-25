using System.Text.Json.Serialization;

namespace Pek.Configuration
{
    /// <summary>
    /// 通用配置JSON序列化上下文基类
    /// 包含常用基本类型的序列化支持
    /// </summary>
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(decimal))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(List<int>))]
    [JsonSerializable(typeof(List<long>))]
    [JsonSerializable(typeof(List<double>))]
    [JsonSerializable(typeof(List<decimal>))]
    [JsonSerializable(typeof(List<bool>))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class ConfigJsonContext : JsonSerializerContext
    {
    }
}