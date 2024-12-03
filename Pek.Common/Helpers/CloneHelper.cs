using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pek.Helpers;

/// <summary>
/// 深度克隆辅助类
/// </summary>
public static class CloneHelper {
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,  // 选项会保留对象的引用信息，以便在反序列化时能够正确处理循环引用
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // 忽略 null 值
    };

    /// <summary>
    /// 深度克隆
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static T? DeepCloneWithJson<T>(this T source)
    {
        // 不要序列化 null 对象，只需返回该对象的默认值
        if (source == null)
        {
            return default;
        }

        var jsonString = JsonSerializer.Serialize(source, _jsonSerializerOptions); // 将对象序列化为 JSON 字符串

        return JsonSerializer.Deserialize<T>(jsonString, _jsonSerializerOptions);  // 将 JSON 字符串反序列化为对象
    }
}