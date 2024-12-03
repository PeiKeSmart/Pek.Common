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
        Converters = { new ObjectToInferredTypesConverter() }
    };

    /// <summary>
    /// 深度克隆
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static T? DeepClone<T>(this T source)
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

public class ObjectToInferredTypesConverter : JsonConverter<Object>
{
    public override Object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.Clone().ToObject();
    }

    public override void Write(Utf8JsonWriter writer, Object value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}

public static class JsonElementExtensions {
    public static Object ToObject(this JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new Dictionary<String, Object>();
                foreach (var property in element.EnumerateObject())
                {
                    obj[property.Name] = property.Value.ToObject();
                }
                return obj;
            case JsonValueKind.Array:
                var array = new List<Object>();
                foreach (var item in element.EnumerateArray())
                {
                    array.Add(item.ToObject());
                }
                return array;
            case JsonValueKind.String:
                return element.GetString()!;
            case JsonValueKind.Number:
                if (element.TryGetInt32(out var intValue))
                    return intValue;
                if (element.TryGetInt64(out var longValue))
                    return longValue;
                return element.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return null!;
            default:
                throw new NotSupportedException($"Unsupported JsonValueKind: {element.ValueKind}");
        }
    }
}
