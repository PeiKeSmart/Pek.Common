# 配置系统

本项目实现了一个线程安全的配置文件系统，类似于 ASP.NET Core 中的标准配置系统。它提供了自动创建配置文件（如果不存在）、为缺失的配置项填充默认值，以及优先使用配置文件中存在的值等功能。

## 功能特性

- **自动文件创建**：如果配置文件不存在，将自动创建。
- **默认值填充**：文件中不存在的配置项将使用默认值填充。
- **文件值优先**：当配置文件中存在值时，将优先使用文件中的值而不是默认值。
- **文件变更监控**：系统监控配置文件的变更并相应地更新内存中的数据。

## 文件结构

- **src/Configuration**：包含核心配置管理类和接口。
  - `ConfigurationManager.cs`：管理配置的加载、更新和保存。
  - `FileConfigurationProvider.cs`：处理配置数据与文件的读写操作。
  - `ConfigurationChangeToken.cs`：表示配置变更的令牌。
  - **Interfaces**：包含配置管理和变更监控的接口。
  
- **src/Serialization**：处理配置对象的序列化和反序列化。
  - `JsonConfigurationSerializer.cs`：将配置对象序列化为 JSON 或从 JSON 反序列化。
  - `IConfigurationSerializer.cs`：配置序列化的接口。

- **src/Extensions**：包含用于简化配置管理操作的扩展方法。
  - `ConfigurationExtensions.cs`：提供配置处理的附加方法。

- **src/Models**：定义配置选项的模型。
  - `ConfigurationOptions.cs`：表示配置选项的模型，包括默认值和类型。

## 使用方法

### 1. 基本使用

#### 步骤一：定义配置类

首先，创建一个配置类来定义您的配置结构：

```csharp
using System.ComponentModel;
using NewLife.Configuration;

[DisplayName("应用程序设置")]
[Config("AppSettings")]
public class AppSettings : Config<AppSettings>
{
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    [Description("数据库连接字符串")]
    public string ConnectionString { get; set; } = "Server=localhost;Database=MyApp;Trusted_Connection=true;";

    /// <summary>
    /// 缓存过期时间（分钟）
    /// </summary>
    [Description("缓存过期时间（分钟）")]
    public int CacheExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// 是否启用调试模式
    /// </summary>
    [Description("是否启用调试模式")]
    public bool EnableDebugMode { get; set; } = false;
}
```

#### 步骤二：使用配置管理器

```csharp
using Configuration;

// 创建配置管理器实例
var configManager = new ConfigurationManager("appsettings.json");

// 加载配置（如果文件不存在，将自动创建）
var appSettings = AppSettings.Current;

// 读取配置值
string connectionString = appSettings.ConnectionString;
int cacheTime = appSettings.CacheExpirationMinutes;
bool debugMode = appSettings.EnableDebugMode;

// 修改配置值
appSettings.EnableDebugMode = true;
appSettings.CacheExpirationMinutes = 60;

// 保存配置（自动保存到文件）
appSettings.Save();
```

### 2. 高级用法

#### 监听配置变更

```csharp
// 订阅配置变更事件
AppSettings.Current.PropertyChanged += (sender, e) =>
{
    Console.WriteLine($"配置项 {e.PropertyName} 已更改");
    
    // 根据具体的配置项执行相应的操作
    switch (e.PropertyName)
    {
        case nameof(AppSettings.ConnectionString):
            // 重新初始化数据库连接
            ReinitializeDatabase();
            break;
        case nameof(AppSettings.CacheExpirationMinutes):
            // 更新缓存策略
            UpdateCachePolicy();
            break;
    }
};
```

#### 使用扩展方法

```csharp
using ConfigurationSystem.Extensions;

// 使用扩展方法加载配置
var defaultConfig = new AppSettings();
configManager.LoadConfiguration("config.json", defaultConfig);

// 获取配置值的类型安全方法
T GetConfigValue<T>(string key, T defaultValue = default)
{
    try
    {
        return configManager.Get<T>(key);
    }
    catch
    {
        return defaultValue;
    }
}
```

### 3. 在 ASP.NET Core 中使用

#### 注册配置服务

```csharp
// Program.cs 或 Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // 注册配置管理器
    services.AddSingleton<IConfigurationManager>(provider =>
        new ConfigurationManager("appsettings.json"));
    
    // 注册配置类
    services.Configure<AppSettings>(config =>
    {
        var appSettings = AppSettings.Current;
        config.ConnectionString = appSettings.ConnectionString;
        config.CacheExpirationMinutes = appSettings.CacheExpirationMinutes;
        config.EnableDebugMode = appSettings.EnableDebugMode;
    });
}
```

#### 在控制器中使用

```csharp
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfigurationManager _configManager;
    private readonly IOptionsMonitor<AppSettings> _appSettings;

    public ConfigController(
        IConfigurationManager configManager,
        IOptionsMonitor<AppSettings> appSettings)
    {
        _configManager = configManager;
        _appSettings = appSettings;
    }

    [HttpGet("current")]
    public IActionResult GetCurrentConfig()
    {
        return Ok(_appSettings.CurrentValue);
    }

    [HttpPost("update")]
    public IActionResult UpdateConfig([FromBody] AppSettings newSettings)
    {
        var current = AppSettings.Current;
        current.ConnectionString = newSettings.ConnectionString;
        current.CacheExpirationMinutes = newSettings.CacheExpirationMinutes;
        current.EnableDebugMode = newSettings.EnableDebugMode;
        
        current.Save();
        
        return Ok("配置已更新");
    }
}
```

### 4. 线程安全使用

配置系统是线程安全的，可以在多线程环境中安全使用：

```csharp
// 在多线程环境中安全读取配置
public class ThreadSafeService
{
    private readonly AppSettings _settings;

    public ThreadSafeService()
    {
        _settings = AppSettings.Current;
    }

    public async Task ProcessDataAsync()
    {
        // 线程安全地读取配置
        var connectionString = _settings.ConnectionString;
        var cacheTime = _settings.CacheExpirationMinutes;
        
        // 执行业务逻辑...
        await SomeAsyncOperation(connectionString, cacheTime);
    }

    // 线程安全地更新配置
    public void UpdateConfiguration(string newConnectionString)
    {
        lock (_settings)
        {
            _settings.ConnectionString = newConnectionString;
            _settings.Save();
        }
    }
}
```

### 5. 最佳实践

1. **配置文件位置**：建议将配置文件放在应用程序根目录下
2. **默认值**：始终为配置属性提供合理的默认值
3. **验证**：在设置配置值时进行适当的验证
4. **文档**：使用 `Description` 特性为配置项提供清晰的说明
5. **版本控制**：将配置文件模板加入版本控制，但排除包含敏感信息的实际配置文件

### 6. 故障排除

- **文件权限**：确保应用程序对配置文件目录有读写权限
- **JSON 格式**：确保配置文件是有效的 JSON 格式
- **类型转换**：注意配置值的类型转换，特别是数值和布尔值
- **文件锁定**：避免在配置文件被其他进程占用时进行写操作

## 贡献

欢迎贡献！请随时提交 pull request 或为任何建议或改进开启 issue。