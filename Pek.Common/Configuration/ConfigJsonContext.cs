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
    [JsonSerializable(typeof(DateTimeOffset))]
    [JsonSerializable(typeof(TimeSpan))]
    [JsonSerializable(typeof(Guid))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(List<int>))]
    [JsonSerializable(typeof(List<long>))]
    [JsonSerializable(typeof(List<double>))]
    [JsonSerializable(typeof(List<decimal>))]
    [JsonSerializable(typeof(List<bool>))]
    [JsonSerializable(typeof(List<DateTime>))]
    [JsonSerializable(typeof(List<Guid>))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(Dictionary<string, int>))]
    [JsonSerializable(typeof(Dictionary<string, bool>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    // 可空值类型支持
    [JsonSerializable(typeof(bool?))]
    [JsonSerializable(typeof(int?))]
    [JsonSerializable(typeof(long?))]
    [JsonSerializable(typeof(double?))]
    [JsonSerializable(typeof(decimal?))]
    [JsonSerializable(typeof(DateTime?))]
    [JsonSerializable(typeof(DateTimeOffset?))]
    [JsonSerializable(typeof(TimeSpan?))]
    [JsonSerializable(typeof(Guid?))]
    public partial class ConfigJsonContext : JsonSerializerContext
    {
    }
}