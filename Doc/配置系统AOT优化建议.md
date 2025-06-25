# Pek.Common 配置系统AOT优化建议

## 已完成的多环境支持移除

### 移除内容
- 多环境配置示例和文档
- 环境相关的配置切换逻辑
- 复杂的配置覆盖机制

### 简化结果
- 单一配置模式，更适合AOT场景
- 减少运行时复杂性
- 提高配置系统的可预测性

## AOT场景优化建议

### 1. 配置预编译检查
```csharp
// 在编译时验证所有配置类都有对应的JsonSerializerContext
public static class ConfigurationValidator
{
    public static void ValidateAotCompatibility()
    {
        // 确保所有配置类都正确注册了JsonSerializerContext
        var configTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Config<>).MakeGenericType(t)))
            .ToList();
            
        foreach (var configType in configTypes)
        {
            // 验证是否有对应的JsonSerializerContext
            var contextName = $"{configType.Name}JsonContext";
            var contextType = Assembly.GetExecutingAssembly()
                .GetType($"{configType.Namespace}.{contextName}");
                
            if (contextType == null)
            {
                throw new InvalidOperationException(
                    $"配置类 {configType.Name} 缺少对应的 JsonSerializerContext: {contextName}");
            }
        }
    }
}
```

### 2. 配置初始化优化
```csharp
// 在应用启动时预热所有配置
public static class ConfigurationPrewarmer
{
    public static void PrewarmConfigurations()
    {
        // 预热所有已注册的配置类型
        var configTypes = ConfigManager.GetRegisteredConfigTypes();
        
        foreach (var configType in configTypes)
        {
            try
            {
                // 触发配置加载，但不保存
                var currentProperty = configType.GetProperty("Current", 
                    BindingFlags.Public | BindingFlags.Static);
                currentProperty?.GetValue(null);
            }
            catch (Exception ex)
            {
                // 记录预热失败的配置
                Console.WriteLine($"配置预热失败: {configType.Name} - {ex.Message}");
            }
        }
    }
}
```

### 3. 配置文件路径简化
```csharp
// 简化配置文件路径逻辑，避免复杂的环境判断
public static string GetSimpleConfigPath<T>() where T : Config<T>
{
    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
    var configDir = Path.Combine(baseDir, "Config");
    var fileName = $"{typeof(T).Name}.config";
    return Path.Combine(configDir, fileName);
}
```

### 4. 静态配置注册模式
```csharp
// 推荐的AOT友好配置类模式
public class MyConfig : Config<MyConfig>
{
    public string Setting1 { get; set; } = "默认值";
    public int Setting2 { get; set; } = 100;
    
    // AOT兼容的静态构造函数
    static MyConfig()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = MyConfigJsonContext.Default,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        ConfigManager.RegisterConfig<MyConfig>(options);
    }
}

[JsonSerializable(typeof(MyConfig))]
public partial class MyConfigJsonContext : JsonSerializerContext
{
}
```

## 最佳实践

### 1. 避免运行时类型发现
- 不使用反射扫描配置类
- 在静态构造函数中显式注册配置
- 使用编译时已知的类型

### 2. 简化配置结构
- 避免深层嵌套的配置对象
- 使用简单的数据类型
- 减少配置之间的依赖关系

### 3. 配置验证前置
- 在静态构造函数中进行基本验证
- 使用编译时常量作为默认值
- 避免复杂的运行时验证逻辑

### 4. 错误处理简化
- 使用简单的异常处理
- 提供明确的错误信息
- 避免复杂的错误恢复机制

## 结论

通过移除多环境支持，配置系统变得更加简洁和AOT友好。当前的实现已经很好地支持了AOT场景的需求，建议在此基础上进行渐进式优化，而不是进行大幅度的架构改动。