using System.Text.Json;
using NewLife.Log;

namespace Pek.Configuration;

/// <summary>
/// 配置JSON对比工具类
/// 用于比较两个配置对象的JSON表示，并生成详细的属性变更信息
/// </summary>
internal static class ConfigJsonComparer
{
    /// <summary>
    /// 比较两个配置对象并获取属性变更信息（AOT兼容）
    /// </summary>
    /// <param name="oldConfig">旧配置对象</param>
    /// <param name="newConfig">新配置对象</param>
    /// <param name="configType">配置类型</param>
    /// <param name="serializerOptions">JSON序列化选项</param>
    /// <returns>属性变更信息列表</returns>
    public static List<ConfigPropertyChange> GetPropertyChanges(
        object? oldConfig, 
        object newConfig, 
        Type configType, 
        JsonSerializerOptions serializerOptions)
    {
        var changes = new List<ConfigPropertyChange>();
        
        if (oldConfig == null)
        {
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = "Configuration",
                OldValue = "初始配置",
                NewValue = "已加载配置"
            });
            return changes;
        }

        try
        {
            // 使用 JSON 序列化比较（AOT 兼容）
            var oldJson = JsonSerializer.Serialize(oldConfig, configType, serializerOptions);
            var newJson = JsonSerializer.Serialize(newConfig, configType, serializerOptions);
            
            if (oldJson != newJson)
            {
                // 使用 JsonDocument 进行属性级比较（AOT 兼容）
                var propertyChanges = CompareJsonProperties(oldJson, newJson);
                changes.AddRange(propertyChanges);
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = "Configuration",
                OldValue = "比较失败",
                NewValue = "配置已变更"
            });
        }

        return changes;
    }

    /// <summary>
    /// 比较两个配置对象（简化版本，当无法获取序列化选项时使用）
    /// </summary>
    /// <param name="oldConfig">旧配置对象</param>
    /// <param name="newConfig">新配置对象</param>
    /// <returns>属性变更信息列表</returns>
    public static List<ConfigPropertyChange> GetPropertyChangesSimple(object? oldConfig, object newConfig)
    {
        var changes = new List<ConfigPropertyChange>();
        
        if (oldConfig == null)
        {
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = "Configuration",
                OldValue = "初始配置",
                NewValue = "已加载配置"
            });
        }
        else if (!ReferenceEquals(oldConfig, newConfig))
        {
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = "Configuration",
                OldValue = "原始配置",
                NewValue = "已更新配置"
            });
        }

        return changes;
    }

    /// <summary>
    /// 比较两个 JSON 字符串的属性差异（AOT 兼容）
    /// </summary>
    /// <param name="oldJson">旧JSON字符串</param>
    /// <param name="newJson">新JSON字符串</param>
    /// <returns>属性变更信息列表</returns>
    private static List<ConfigPropertyChange> CompareJsonProperties(string oldJson, string newJson)
    {
        var changes = new List<ConfigPropertyChange>();

        try
        {
            using var oldDoc = JsonDocument.Parse(oldJson);
            using var newDoc = JsonDocument.Parse(newJson);

            CompareJsonElements(oldDoc.RootElement, newDoc.RootElement, "", changes);
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = "JSON比较",
                OldValue = "解析失败",
                NewValue = "配置已变更"
            });
        }

        return changes;
    }

    /// <summary>
    /// 递归比较 JSON 元素（AOT 兼容）
    /// </summary>
    /// <param name="oldElement">旧JSON元素</param>
    /// <param name="newElement">新JSON元素</param>
    /// <param name="propertyPath">属性路径</param>
    /// <param name="changes">变更信息列表</param>
    private static void CompareJsonElements(JsonElement oldElement, JsonElement newElement, string propertyPath, List<ConfigPropertyChange> changes)
    {
        if (oldElement.ValueKind != newElement.ValueKind)
        {
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = propertyPath,
                OldValue = GetJsonElementValueAsString(oldElement),
                NewValue = GetJsonElementValueAsString(newElement)
            });
            return;
        }

        switch (oldElement.ValueKind)
        {
            case JsonValueKind.Object:
                CompareJsonObjects(oldElement, newElement, propertyPath, changes);
                break;
            
            case JsonValueKind.Array:
                CompareJsonArrays(oldElement, newElement, propertyPath, changes);
                break;
            
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                var oldValue = GetJsonElementValueAsString(oldElement);
                var newValue = GetJsonElementValueAsString(newElement);
                if (!Equals(oldValue, newValue))
                {
                    changes.Add(new ConfigPropertyChange
                    {
                        PropertyName = propertyPath,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                }
                break;
        }
    }

    /// <summary>
    /// 比较 JSON 对象
    /// </summary>
    /// <param name="oldObj">旧JSON对象</param>
    /// <param name="newObj">新JSON对象</param>
    /// <param name="basePath">基础路径</param>
    /// <param name="changes">变更信息列表</param>
    private static void CompareJsonObjects(JsonElement oldObj, JsonElement newObj, string basePath, List<ConfigPropertyChange> changes)
    {
        var oldProperties = new Dictionary<string, JsonElement>();
        var newProperties = new Dictionary<string, JsonElement>();

        // 收集旧对象的属性
        foreach (var prop in oldObj.EnumerateObject())
        {
            oldProperties[prop.Name] = prop.Value;
        }

        // 收集新对象的属性
        foreach (var prop in newObj.EnumerateObject())
        {
            newProperties[prop.Name] = prop.Value;
        }

        // 检查所有属性
        var allPropertyNames = oldProperties.Keys.Union(newProperties.Keys);
        foreach (var propName in allPropertyNames)
        {
            var propertyPath = string.IsNullOrEmpty(basePath) ? propName : $"{basePath}.{propName}";

            if (!oldProperties.TryGetValue(propName, out var oldProp))
            {
                // 新增属性
                changes.Add(new ConfigPropertyChange
                {
                    PropertyName = propertyPath,
                    OldValue = "null",
                    NewValue = GetJsonElementValueAsString(newProperties[propName])
                });
            }
            else if (!newProperties.TryGetValue(propName, out var newProp))
            {
                // 删除属性
                changes.Add(new ConfigPropertyChange
                {
                    PropertyName = propertyPath,
                    OldValue = GetJsonElementValueAsString(oldProp),
                    NewValue = "null"
                });
            }
            else
            {
                // 比较属性值
                CompareJsonElements(oldProp, newProp, propertyPath, changes);
            }
        }
    }

    /// <summary>
    /// 比较 JSON 数组
    /// </summary>
    /// <param name="oldArray">旧JSON数组</param>
    /// <param name="newArray">新JSON数组</param>
    /// <param name="propertyPath">属性路径</param>
    /// <param name="changes">变更信息列表</param>
    private static void CompareJsonArrays(JsonElement oldArray, JsonElement newArray, string propertyPath, List<ConfigPropertyChange> changes)
    {
        var oldItems = oldArray.EnumerateArray().ToArray();
        var newItems = newArray.EnumerateArray().ToArray();

        if (oldItems.Length != newItems.Length)
        {
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = $"{propertyPath}.Length",
                OldValue = oldItems.Length.ToString(),
                NewValue = newItems.Length.ToString()
            });
        }

        var maxLength = Math.Max(oldItems.Length, newItems.Length);
        for (var i = 0; i < maxLength; i++)
        {
            var itemPath = $"{propertyPath}[{i}]";
            
            if (i >= oldItems.Length)
            {
                changes.Add(new ConfigPropertyChange
                {
                    PropertyName = itemPath,
                    OldValue = "null",
                    NewValue = GetJsonElementValueAsString(newItems[i])
                });
            }
            else if (i >= newItems.Length)
            {
                changes.Add(new ConfigPropertyChange
                {
                    PropertyName = itemPath,
                    OldValue = GetJsonElementValueAsString(oldItems[i]),
                    NewValue = "null"
                });
            }
            else
            {
                CompareJsonElements(oldItems[i], newItems[i], itemPath, changes);
            }
        }
    }

    /// <summary>
    /// 获取 JSON 元素的字符串值（统一返回string类型以避免类型二义性）
    /// </summary>
    /// <param name="element">JSON元素</param>
    /// <returns>元素的字符串值</returns>
    private static string GetJsonElementValueAsString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "null",
            JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal.ToString() : element.GetDouble().ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            JsonValueKind.Array => $"Array[{element.GetArrayLength()}]",
            JsonValueKind.Object => "Object",
            _ => element.ToString()
        };
    }
}