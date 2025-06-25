# Pek.Configuration 配置系统

## 概述

`Pek.Configuration` 是一个简单易用的配置管理系统，支持 JSON 格式的配置文件，并提供了 AOT 兼容的序列化支持。配置系统会自动在应用程序根目录下的 `Config` 文件夹中查找或创建配置文件。

## 特性

- **自动初始化**：配置类在首次访问时会自动初始化，无需显式调用初始化方法
- **AOT 兼容**：支持 .NET Native 和 Blazor WebAssembly 等 AOT 编译环境
- **类型安全**：使用泛型和强类型配置类
- **线程安全**：支持多线程环境下的配置访问
- **自动序列化/反序列化**：使用 System.Text.Json
- **懒加载**：配置只在首次访问时加载，提高性能

## 使用方法

### 1. 创建配置类

```csharp
using System.Text.Json.Serialization;
using Pek.Configuration;

public class AppConfig : Config<AppConfig>
{
    public string ApiUrl { get; set; } = "https://api.example.com";
    public int MaxRetries { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
    
    // 静态构造函数，注册配置
    static AppConfig()
    {
        RegisterConfigForAot(AppConfigJsonContext.Default);
    }
}

// AOT兼容的JSON序列化上下文
[JsonSerializable(typeof(AppConfig))]
public partial class AppConfigJsonContext : ConfigJsonContext
{
}
```

### 2. 访问配置

```csharp
// 获取配置实例（首次访问时会自动初始化）
var config = AppConfig.Current;

// 使用配置
var apiUrl = config.ApiUrl;
var maxRetries = config.MaxRetries;

// 修改配置
config.EnableLogging = false;

// 保存配置
config.Save();

// 重新加载配置
AppConfig.Reload();
```

### 3. 配置自动初始化机制

配置类在首次访问时会自动初始化，通常不需要显式调用初始化方法。这是通过以下机制实现的：

1. 当首次访问 `Config<T>.Current` 属性时，会调用 `EnsureInitialized()` 方法
2. `EnsureInitialized()` 方法会检查配置类是否已初始化，如果未初始化，则使用 `RuntimeHelpers.RunClassConstructor()` 触发静态构造函数
3. 静态构造函数中调用 `RegisterConfigForAot()` 方法注册配置类的序列化选项
4. 初始化完成后，设置标志位，确保静态构造函数只被调用一次

**重要说明**：此机制不使用反射，而是通过直接访问 `Current` 属性来触发初始化逻辑，确保在AOT环境下也能正常工作。

这种机制确保了：
- 配置类只在需要时才初始化（懒加载）
- 静态构造函数只被调用一次
- AOT 兼容性得到保证
- 无需手动初始化
- 不依赖反射，性能更好

如果需要预先初始化特定配置，可以使用以下方法：

```csharp
// 初始化特定的配置类
ConfigInitializer.InitializeConfig<AppConfig>();

// 或者直接访问配置的Current属性也可以触发初始化
var config = AppConfig.Current;
```

但在大多数情况下，您不需要显式调用这些方法，因为配置会在首次访问时自动初始化。

## 高级用法

### 自定义配置文件名

```csharp
static AppConfig()
{
    RegisterConfigForAot(AppConfigJsonContext.Default, "CustomAppSettings");
}
```

### 自定义序列化选项

```csharp
static AppConfig()
{
    RegisterConfigForAot(AppConfigJsonContext.Default, writeIndented: true, useCamelCase: false);
}
```

## 注意事项

1. 每个配置类必须有一个静态构造函数，调用 `RegisterConfigForAot` 方法注册配置
2. 每个配置类必须有一个对应的 `JsonContext` 类，继承自 `ConfigJsonContext`
3. 配置类必须有一个无参构造函数
4. 配置文件默认保存在应用程序根目录下的 `Config` 文件夹中