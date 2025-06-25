# Pek.Common 配置系统 AOT 兼容性指南

## 什么是 AOT 编译？

AOT (Ahead-of-Time) 编译是一种编译技术，它在程序运行前将代码编译成本地机器码，而不是在运行时进行即时编译 (JIT)。AOT 编译的主要优势包括：

- **更快的启动时间**：无需在运行时进行 JIT 编译
- **更小的内存占用**：不需要 JIT 编译器
- **更好的性能**：代码已经优化为本地机器码
- **更好的安全性**：减少了运行时代码生成

然而，AOT 编译也带来了一些限制，特别是对反射和动态代码生成的限制，这会影响到配置系统的使用。

## AOT 环境中的挑战

在 AOT 环境中使用配置系统面临以下挑战：

1. **反射限制**：AOT 编译环境限制了运行时反射的使用
2. **动态代码生成限制**：无法在运行时生成序列化/反序列化代码
3. **类型信息丢失**：某些类型信息在 AOT 编译后可能丢失

## 解决方案：源代码生成

Pek.Common 配置系统使用 System.Text.Json 的源代码生成功能来解决 AOT 兼容性问题。源代码生成在编译时生成序列化/反序列化代码，而不是在运行时使用反射。

### 步骤 1：创建 JsonSerializerContext

为每个配置类创建一个 JsonSerializerContext 派生类，并使用 [JsonSerializable] 特性标记所有需要序列化的类型：

```csharp
[JsonSerializable(typeof(MyConfig))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
// 添加所有需要序列化的类型
public partial class MyConfigJsonContext : JsonSerializerContext
{
}
```

注意：
- 类必须标记为 `partial`
- 必须包含配置类本身和所有配置属性的类型
- 对于集合类型，还需要包含集合元素的类型

### 步骤 2：配置 JsonSerializerOptions

在注册配置时，提供正确的 JsonSerializerOptions，并设置 TypeInfoResolver：

```csharp
var options = new JsonSerializerOptions
{
    TypeInfoResolver = MyConfigJsonContext.Default,
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    // 其他选项...
};

ConfigManager.RegisterConfig<MyConfig>(options);
```

### 步骤 3：预初始化配置类

在应用程序启动时预初始化所有配置类，确保静态构造函数被调用：

```csharp
private static void InitializeConfigs()
{
    try
    {
        // 预先初始化MyConfig
        var jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = MyConfigJsonContext.Default,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        // 手动注册配置
        ConfigManager.RegisterConfig<MyConfig>(jsonOptions);
        
        // 访问配置触发初始化
        var config = MyConfig.Current;
        Console.WriteLine("✅ MyConfig初始化成功");
        
        // 初始化其他配置类...
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ 配置初始化失败: {ex.Message}");
    }
}
```

## 完整示例

### 配置类定义

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyApp.Configuration;

// 1. 创建JsonSerializerContext
[JsonSerializable(typeof(ServerConfig))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(List<string>))]
public partial class ServerConfigJsonContext : JsonSerializerContext
{
}

// 2. 创建配置类
public class ServerConfig : Config<ServerConfig>
{
    public string? ServerName { get; set; } = "DefaultServer";
    public string? IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8080;
    public bool EnableSsl { get; set; } = false;
    public List<string> AllowedClients { get; set; } = new List<string>();
    
    // 3. 静态构造函数
    static ServerConfig()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = ServerConfigJsonContext.Default,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        ConfigManager.RegisterConfig<ServerConfig>(options);
    }
}
```

### 程序启动初始化

```csharp
public static class Program
{
    public static void Main(string[] args)
    {
        // 初始化配置系统
        InitializeConfigs();
        
        // 应用程序逻辑
        var config = ServerConfig.Current;
        Console.WriteLine($"服务器名称: {config.ServerName}");
        Console.WriteLine($"IP地址: {config.IpAddress}");
        Console.WriteLine($"端口: {config.Port}");
    }
    
    private static void InitializeConfigs()
    {
        try
        {
            // 预先初始化ServerConfig
            var jsonOptions = new JsonSerializerOptions
            {
                TypeInfoResolver = ServerConfigJsonContext.Default,
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            
            // 手动注册配置
            ConfigManager.RegisterConfig<ServerConfig>(jsonOptions);
            
            // 访问配置触发初始化
            var config = ServerConfig.Current;
            Console.WriteLine("✅ 配置系统初始化成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ 配置系统初始化失败: {ex.Message}");
            // 记录详细错误信息
            Console.WriteLine($"详细错误: {ex}");
        }
    }
}
```

## 常见问题与解决方案

### 1. 序列化/反序列化错误

**问题**：在 AOT 环境中出现序列化或反序列化错误。

**解决方案**：
- 确保所有需要序列化的类型都在 JsonSerializerContext 中注册
- 检查复杂类型（如集合、嵌套对象）是否也已注册

### 2. 静态构造函数未被调用

**问题**：配置类的静态构造函数未被调用，导致配置未注册。

**解决方案**：
- 在程序启动时显式访问配置类的 Current 属性
- 使用本文档中的预初始化方法

### 3. 配置属性类型不兼容

**问题**：某些类型在 AOT 环境中可能不支持序列化。

**解决方案**：
- 避免使用复杂的自定义类型作为配置属性
- 对于必须使用的复杂类型，确保它们也有对应的 JsonSerializerContext

### 4. 版本兼容性问题

**问题**：不同版本的 System.Text.Json 可能有不同的 API。

**解决方案**：
- 在 JsonSerializerOptions 中只使用所有目标版本都支持的属性
- 针对不同的目标框架使用条件编译指令

```csharp
var options = new JsonSerializerOptions
{
    TypeInfoResolver = MyConfigJsonContext.Default,
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
#if NET6_0_OR_GREATER
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
#endif
};
```

## 总结

通过遵循本指南中的步骤，可以确保 Pek.Common 配置系统在 AOT 编译环境中正常工作。关键点包括：

1. 为每个配置类创建 JsonSerializerContext
2. 注册所有需要序列化的类型
3. 在程序启动时预初始化配置类
4. 处理可能出现的异常

这些措施可以确保配置系统在各种环境中都能可靠工作，包括 .NET Native、Xamarin AOT、Blazor WebAssembly 和 .NET 8+ 的 NativeAOT 模式。