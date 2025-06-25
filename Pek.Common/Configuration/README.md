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
- **智能自动重新加载**：支持配置文件外部修改后自动更新内存中的配置对象，同时过滤代码保存操作
- **通用配置变更通知**：提供统一的事件管理机制，支持多种订阅方式
- **防抖处理**：避免频繁的文件变更导致多次重新加载
- **新旧值比较**：配置变更事件提供新旧配置值对比功能

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
public partial class AppConfigJsonContext : JsonSerializerContext
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

### 3. 通用配置变更事件（推荐方式）

配置系统提供了统一的事件管理机制，您无需为每个配置类单独设置事件订阅：

#### 3.1 订阅特定类型的配置变更

```csharp
// 订阅应用配置变更
ConfigManager.SubscribeConfigChanged<AppConfig>(newConfig =>
{
    Console.WriteLine($"应用配置已更新: {newConfig.ApiUrl}");
    // 重新初始化HTTP客户端
    // 更新日志配置
});

// 订阅数据库配置变更  
ConfigManager.SubscribeConfigChanged<DatabaseConfig>(newConfig =>
{
    Console.WriteLine($"数据库配置已更新: {newConfig.ConnectionString}");
    // 重新初始化数据库连接池
});
```

#### 3.2 订阅配置变更并比较新旧值

```csharp
ConfigManager.SubscribeConfigChanged<AppConfig>((oldConfig, newConfig) =>
{
    // 比较具体的属性变更
    if (oldConfig.ApiUrl != newConfig.ApiUrl)
    {
        Console.WriteLine($"API地址变更: {oldConfig.ApiUrl} → {newConfig.ApiUrl}");
        // 重新初始化HTTP客户端
        HttpClientManager.Reinitialize(newConfig.ApiUrl);
    }
    
    if (oldConfig.MaxRetries != newConfig.MaxRetries)
    {
        Console.WriteLine($"重试次数变更: {oldConfig.MaxRetries} → {newConfig.MaxRetries}");
        // 更新重试策略
        RetryPolicy.UpdateMaxRetries(newConfig.MaxRetries);
    }
});
```

#### 3.3 订阅所有配置类型的变更

```csharp
ConfigManager.SubscribeAllConfigChanged(e =>
{
    Console.WriteLine($"配置 {e.ConfigName} 在 {DateTime.Now:HH:mm:ss} 发生变更");
    
    // 记录配置变更日志
    Logger.LogInformation($"Configuration {e.ConfigName} has been updated");
    
    // 发送变更通知
    NotificationService.SendConfigChangeNotification(e.ConfigName);
    
    // 更新监控指标
    Metrics.IncrementConfigChangeCounter(e.ConfigName);
});
```

#### 3.4 统一配置事件管理

```csharp
// 推荐的做法：在应用程序启动时统一设置
public static void SetupConfigurationEventHandlers()
{
    // 应用配置变更处理
    ConfigManager.SubscribeConfigChanged<AppConfig>(config =>
    {
        // 更新HTTP客户端配置
        HttpClientFactory.UpdateConfiguration(config);
        // 更新日志级别
        LogManager.UpdateLogLevel(config.EnableLogging);
        // 清除相关缓存
        CacheManager.ClearCache("api-cache");
    });

    // 数据库配置变更处理
    ConfigManager.SubscribeConfigChanged<DatabaseConfig>(config =>
    {
        // 重新初始化数据库连接池
        DatabaseManager.ReconfigureConnectionPool(config);
        // 更新ORM配置
        EntityFrameworkConfig.UpdateConfiguration(config);
    });

    // 缓存配置变更处理
    ConfigManager.SubscribeConfigChanged<CacheConfig>(config =>
    {
        // 重新配置缓存
        CacheManager.Reconfigure(config);
    });

    // 全局配置变更监控
    ConfigManager.SubscribeAllConfigChanged(e =>
    {
        // 记录变更审计日志
        AuditLogger.LogConfigurationChange(e.ConfigName, e.OldConfig, e.NewConfig);
        // 发送变更通知给管理员
        AdminNotifier.NotifyConfigurationChange(e);
    });
}
```

### 4. 智能自动重新加载功能

配置系统默认启用智能自动重新加载功能，具有以下特点：

#### 4.1 智能过滤机制

- **代码保存过滤**：当通过 `config.Save()` 保存配置时，不会触发自动重新加载事件
- **外部修改检测**：只有外部手动修改配置文件才会触发自动重新加载
- **防抖处理**：使用 500 毫秒的防抖间隔，避免频繁的文件变更触发多次重新加载
- **保存忽略间隔**：代码保存后 1000 毫秒内的文件变更会被忽略

#### 4.2 工作原理

1. **文件监控**：系统自动监控 `Config` 文件夹中的 `.config` 文件
2. **变更检测**：检测到文件变更时，首先判断是否为代码保存导致
3. **智能过滤**：过滤掉代码保存，只处理外部手动修改
4. **异步更新**：异步重新加载配置并更新内存对象
5. **事件通知**：触发相应的配置变更事件

#### 4.3 手动修改配置文件示例

假设您有以下配置文件 `Config/AppConfig.config`：

```json
{
  "apiUrl": "https://api.example.com",
  "maxRetries": 3,
  "enableLogging": true
}
```

当您手动将其修改为：

```json
{
  "apiUrl": "https://newapi.example.com",
  "maxRetries": 5,
  "enableLogging": false
}
```

系统会自动：
1. 检测到外部文件变更（排除代码保存）
2. 重新加载配置文件
3. 更新 `AppConfig.Current` 中的值
4. 触发所有订阅的配置变更事件

#### 4.4 自动重新加载功能

配置系统的自动重新加载功能始终启用，无需手动控制。系统会：

- **智能监控**：自动监控配置文件变更
- **防抖处理**：避免频繁触发重新加载
- **代码保存过滤**：过滤掉程序代码保存导致的变更
- **异步处理**：异步重新加载配置，不阻塞主线程

### 5. 配置自动初始化机制

配置类在首次访问时会自动初始化，无需显式调用初始化方法：

1. 当首次访问 `Config<T>.Current` 属性时，会调用 `EnsureInitialized()` 方法
2. `EnsureInitialized()` 检查配置类是否已初始化，如果未初始化，则使用 `RuntimeHelpers.RunClassConstructor()` 触发静态构造函数
3. 静态构造函数中调用 `RegisterConfigForAot()` 方法注册配置类的序列化选项
4. 初始化完成后，设置标志位，确保静态构造函数只被调用一次

**重要说明**：此机制不使用反射，确保在AOT环境下也能正常工作。

这种机制确保了：
- 配置类只在需要时才初始化（懒加载）
- 静态构造函数只被调用一次
- AOT 兼容性得到保证
- 无需手动初始化
- 不依赖反射，性能更好

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

### 取消配置变更事件订阅

```csharp
// 创建事件处理程序
EventHandler<ConfigChangedEventArgs> handler = (sender, e) =>
{
    Console.WriteLine($"配置 {e.ConfigName} 已变更");
};

// 订阅事件
ConfigManager.AnyConfigChanged += handler;

// 取消订阅
ConfigManager.UnsubscribeConfigChanged(handler);
```

## 完整示例

```csharp
using System;
using System.Text.Json.Serialization;
using Pek.Configuration;

// 应用程序启动
public class Program
{
    public static void Main(string[] args)
    {
        // 设置配置变更事件处理
        SetupConfigurationEventHandlers();
        
        // 获取配置并使用
        var appConfig = AppConfig.Current;
        var dbConfig = DatabaseConfig.Current;
        
        Console.WriteLine($"API URL: {appConfig.ApiUrl}");
        Console.WriteLine($"DB Connection: {dbConfig.ConnectionString}");
        
        // 应用程序逻辑...
        
        Console.ReadKey();
    }
    
    private static void SetupConfigurationEventHandlers()
    {
        // 订阅应用配置变更
        ConfigManager.SubscribeConfigChanged<AppConfig>((oldConfig, newConfig) =>
        {
            if (oldConfig.ApiUrl != newConfig.ApiUrl)
            {
                // 重新配置HTTP客户端
                HttpClientManager.UpdateBaseUrl(newConfig.ApiUrl);
            }
        });
        
        // 订阅数据库配置变更
        ConfigManager.SubscribeConfigChanged<DatabaseConfig>(newConfig =>
        {
            // 重新配置数据库连接
            DatabaseManager.UpdateConnectionString(newConfig.ConnectionString);
        });
        
        // 监控所有配置变更
        ConfigManager.SubscribeAllConfigChanged(e =>
        {
            Logger.LogInformation($"Configuration {e.ConfigName} updated at {DateTime.Now}");
        });
    }
}

// 配置类定义
public class AppConfig : Config<AppConfig>
{
    public string ApiUrl { get; set; } = "https://api.example.com";
    public int MaxRetries { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
    
    static AppConfig()
    {
        RegisterConfigForAot(AppConfigJsonContext.Default);
    }
}

[JsonSerializable(typeof(AppConfig))]
public partial class AppConfigJsonContext : JsonSerializerContext
{
}

public class DatabaseConfig : Config<DatabaseConfig>
{
    public string ConnectionString { get; set; } = "Server=localhost;Database=MyApp;";
    public int MaxConnections { get; set; } = 50;
    public bool EnableSqlLogging { get; set; } = false;
    
    static DatabaseConfig()
    {
        RegisterConfigForAot(DatabaseConfigJsonContext.Default);
    }
}

[JsonSerializable(typeof(DatabaseConfig))]
public partial class DatabaseConfigJsonContext : JsonSerializerContext
{
}
```

## 最佳实践

1. **统一事件管理**：在应用程序启动时统一设置所有配置变更事件处理
2. **类型安全订阅**：使用泛型方法订阅特定类型的配置变更
3. **新旧值比较**：利用新旧值比较功能，只在特定属性变更时执行相应逻辑
4. **资源清理**：在配置变更时及时清理和重新初始化相关资源
5. **异常处理**：在配置变更事件处理中添加适当的异常处理
6. **日志记录**：记录配置变更的审计日志，便于问题排查

## 注意事项

1. 每个配置类必须有一个静态构造函数，调用 `RegisterConfigForAot` 方法注册配置
2. 每个配置类必须有一个对应的 `JsonContext` 类，继承自 `JsonSerializerContext`
3. 配置类必须有一个无参构造函数
4. 配置文件默认保存在应用程序根目录下的 `Config` 文件夹中
5. 自动重新加载功能默认启用，智能过滤代码保存操作
6. 建议使用通用的配置变更事件管理机制，而不是为每个配置类单独设置事件
7. 配置变更事件在主线程上执行，避免在事件处理中执行耗时操作
8. 取消事件订阅时要确保使用相同的事件处理程序引用