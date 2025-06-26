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
    /// 用于变更追踪的配置快照（深拷贝）
    /// </summary>
    protected string? _configSnapshot;

    /// <summary>
    /// 创建当前配置的快照（用于变更追踪）
    /// </summary>
    public void CreateSnapshot()
    {
        try
        {
            var configType = GetType();
            if (ConfigManager.TryGetSerializerOptions(configType, out var options))
            {
                _configSnapshot = JsonSerializer.Serialize(this, configType, options);
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            _configSnapshot = null;
        }
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public virtual void Save()
    {
        // 获取保存前的配置状态（从快照恢复）
        object? oldConfig = null;
        try
        {
            if (!string.IsNullOrEmpty(_configSnapshot))
            {
                var configType = GetType();
                if (ConfigManager.TryGetSerializerOptions(configType, out var options))
                {
                    oldConfig = JsonSerializer.Deserialize(_configSnapshot, configType, options);
                }
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }

        // 保存配置
        ConfigManager.SaveConfig(this);
        
        // 记录变更日志
        LogConfigChanges(GetType().Name, oldConfig, this);
        
        // 更新快照为当前状态
        CreateSnapshot();
    }

    /// <summary>
    /// 获取配置变更信息（子类可重写以提供更精确的变更检测）
    /// </summary>
    /// <param name="oldConfig">旧配置对象</param>
    /// <returns>配置变更列表</returns>
    protected virtual List<ConfigPropertyChange> GetPropertyChanges(object? oldConfig)
    {
        if (oldConfig == null) return new List<ConfigPropertyChange>();
        
        // 使用JSON序列化进行对比（AOT兼容）
        var configType = GetType();
        var changes = new List<ConfigPropertyChange>();
        
        try
        {
            // 获取序列化选项
            if (ConfigManager.TryGetSerializerOptions(configType, out var options))
            {
                // 序列化两个配置对象
                var oldJson = JsonSerializer.Serialize(oldConfig, configType, options);
                var newJson = JsonSerializer.Serialize(this, configType, options);
                
                // 如果JSON不同，则表示有变更
                if (oldJson != newJson)
                {
                    // 使用JsonDocument进行简单的差异检测
                    changes = CompareJsonDocuments(oldJson, newJson);
                }
            }
            else
            {
                // 回退到简单的对象比较
                if (!ReferenceEquals(oldConfig, this))
                {
                    changes.Add(new ConfigPropertyChange
                    {
                        PropertyName = "Configuration",
                        OldValue = "Previous state",
                        NewValue = "Current state"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            // 发生异常时返回通用变更信息
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = "Configuration",
                OldValue = "Error detecting changes",
                NewValue = "Configuration updated"
            });
        }
        
        return changes;
    }

    /// <summary>
    /// 比较两个JSON文档的差异（AOT兼容）
    /// </summary>
    private List<ConfigPropertyChange> CompareJsonDocuments(string oldJson, string newJson)
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
            // 如果解析失败，返回通用变更信息
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = "Configuration",
                OldValue = "Previous configuration",
                NewValue = "Updated configuration"
            });
        }
        
        return changes;
    }

    /// <summary>
    /// 递归比较JSON元素（AOT兼容）
    /// </summary>
    private void CompareJsonElements(JsonElement oldElement, JsonElement newElement, string path, List<ConfigPropertyChange> changes)
    {
        // 限制变更记录数量，避免过多输出
        if (changes.Count >= 10) return;
        
        if (oldElement.ValueKind != newElement.ValueKind)
        {
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = string.IsNullOrEmpty(path) ? "Root" : path,
                OldValue = GetJsonElementValue(oldElement),
                NewValue = GetJsonElementValue(newElement)
            });
            return;
        }

        switch (oldElement.ValueKind)
        {
            case JsonValueKind.Object:
                CompareJsonObjects(oldElement, newElement, path, changes);
                break;
            case JsonValueKind.Array:
                CompareJsonArrays(oldElement, newElement, path, changes);
                break;
            default:
                if (!JsonElementEquals(oldElement, newElement))
                {
                    changes.Add(new ConfigPropertyChange
                    {
                        PropertyName = string.IsNullOrEmpty(path) ? "Root" : path,
                        OldValue = GetJsonElementValue(oldElement),
                        NewValue = GetJsonElementValue(newElement)
                    });
                }
                break;
        }
    }

    /// <summary>
    /// 比较JSON对象（AOT兼容）
    /// </summary>
    private void CompareJsonObjects(JsonElement oldObj, JsonElement newObj, string basePath, List<ConfigPropertyChange> changes)
    {
        // 获取所有属性名
        var oldProps = oldObj.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
        var newProps = newObj.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
        
        // 检查变更和新增的属性
        foreach (var newProp in newProps)
        {
            if (changes.Count >= 10) break;
            
            var propPath = string.IsNullOrEmpty(basePath) ? newProp.Key : $"{basePath}.{newProp.Key}";
            
            if (oldProps.TryGetValue(newProp.Key, out var oldValue))
            {
                CompareJsonElements(oldValue, newProp.Value, propPath, changes);
            }
            else
            {
                // 新增属性
                changes.Add(new ConfigPropertyChange
                {
                    PropertyName = propPath,
                    OldValue = "null",
                    NewValue = GetJsonElementValue(newProp.Value)
                });
            }
        }
        
        // 检查删除的属性
        foreach (var oldProp in oldProps)
        {
            if (changes.Count >= 10) break;
            
            if (!newProps.ContainsKey(oldProp.Key))
            {
                var propPath = string.IsNullOrEmpty(basePath) ? oldProp.Key : $"{basePath}.{oldProp.Key}";
                changes.Add(new ConfigPropertyChange
                {
                    PropertyName = propPath,
                    OldValue = GetJsonElementValue(oldProp.Value),
                    NewValue = "null"
                });
            }
        }
    }

    /// <summary>
    /// 比较JSON数组（AOT兼容）
    /// </summary>
    private void CompareJsonArrays(JsonElement oldArray, JsonElement newArray, string basePath, List<ConfigPropertyChange> changes)
    {
        var oldItems = oldArray.EnumerateArray().ToArray();
        var newItems = newArray.EnumerateArray().ToArray();
        
        if (oldItems.Length != newItems.Length)
        {
            changes.Add(new ConfigPropertyChange
            {
                PropertyName = $"{basePath}.Length",
                OldValue = oldItems.Length.ToString(),
                NewValue = newItems.Length.ToString()
            });
            return; // 长度不同就不再详细比较元素
        }
        
        // 简单比较前几个元素
        var maxCompare = Math.Min(oldItems.Length, 3);
        for (var i = 0; i < maxCompare && changes.Count < 10; i++)
        {
            CompareJsonElements(oldItems[i], newItems[i], $"{basePath}[{i}]", changes);
        }
    }

    /// <summary>
    /// 判断两个JSON元素是否相等（AOT兼容）
    /// </summary>
    private bool JsonElementEquals(JsonElement element1, JsonElement element2)
    {
        if (element1.ValueKind != element2.ValueKind)
            return false;

        return element1.ValueKind switch
        {
            JsonValueKind.String => element1.GetString() == element2.GetString(),
            JsonValueKind.Number => element1.GetDecimal() == element2.GetDecimal(),
            JsonValueKind.True or JsonValueKind.False => element1.GetBoolean() == element2.GetBoolean(),
            JsonValueKind.Null => true,
            _ => element1.GetRawText() == element2.GetRawText()
        };
    }

    /// <summary>
    /// 获取JSON元素的字符串值（AOT兼容）
    /// </summary>
    private string GetJsonElementValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "null",
            JsonValueKind.Number => element.GetDecimal().ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            JsonValueKind.Array => $"[{element.GetArrayLength()} items]",
            JsonValueKind.Object => "{object}",
            _ => element.GetRawText()
        };
    }

    /// <summary>
    /// 记录配置变更日志
    /// </summary>
    /// <param name="configName">配置名称</param>
    /// <param name="oldConfig">旧配置</param>
    /// <param name="newConfig">新配置</param>
    private void LogConfigChanges(string configName, object? oldConfig, object newConfig)
    {
        try
        {
            var changes = GetPropertyChanges(oldConfig);
            if (changes.Count > 0)
            {
                XTrace.WriteLine($"[CONFIG-SAVE] {configName} 配置保存变更:");
                foreach (var change in changes.Take(5)) // 限制显示前5个变更
                {
                    XTrace.WriteLine($"  • {change.PropertyName}: {change.OldValue} → {change.NewValue}");
                }
                
                if (changes.Count > 5)
                {
                    XTrace.WriteLine($"  ... 还有 {changes.Count - 5} 个属性变更");
                }
            }
            else
            {
                XTrace.WriteLine($"[CONFIG-SAVE] {configName} 配置已保存（无变更）");
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            XTrace.WriteLine($"[CONFIG-SAVE] {configName} 配置已保存（变更检测失败）");
        }
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
            var config = ConfigManager.GetConfig<TConfig>();
            
            // 确保配置实例有快照（用于变更追踪）
            if (config._configSnapshot == null)
            {
                config.CreateSnapshot();
            }
            
            return config;
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
        var config = ConfigManager.GetConfig<TConfig>(forceReload: true);
        
        // 重新加载后创建新的快照
        config.CreateSnapshot();
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

/// <summary>
/// 配置属性变更信息
/// </summary>
public class ConfigPropertyChange
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName { get; set; } = null!;

    /// <summary>
    /// 旧值
    /// </summary>
    public string OldValue { get; set; } = null!;

    /// <summary>
    /// 新值
    /// </summary>
    public string NewValue { get; set; } = null!;
}