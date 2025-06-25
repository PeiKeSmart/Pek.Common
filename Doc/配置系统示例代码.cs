using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pek.Configuration;

namespace Pek.Examples.Configuration
{
    #region 自定义配置类示例
    
    /// <summary>
    /// 数据库配置的JSON序列化上下文（AOT兼容）
    /// </summary>
    [JsonSerializable(typeof(DatabaseConfig))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(ConnectionMode))]
    public partial class DatabaseConfigJsonContext : JsonSerializerContext
    {
    }
    
    /// <summary>
    /// 连接模式枚举
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// 标准模式
        /// </summary>
        Standard,
        
        /// <summary>
        /// 高性能模式
        /// </summary>
        HighPerformance,
        
        /// <summary>
        /// 高可靠性模式
        /// </summary>
        HighReliability
    }
    
    /// <summary>
    /// 数据库配置类
    /// </summary>
    public class DatabaseConfig : Config<DatabaseConfig>
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string? ConnectionString { get; set; } = "Server=localhost;Database=MyDb;User Id=sa;Password=password;";
        
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxConnections { get; set; } = 100;
        
        /// <summary>
        /// 连接超时（秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;
        
        /// <summary>
        /// 是否启用连接池
        /// </summary>
        public bool EnableConnectionPooling { get; set; } = true;
        
        /// <summary>
        /// 连接模式
        /// </summary>
        public ConnectionMode Mode { get; set; } = ConnectionMode.Standard;
        
        /// <summary>
        /// 允许的IP地址列表
        /// </summary>
        public List<string> AllowedIpAddresses { get; set; } = new List<string> { "127.0.0.1" };
        
        /// <summary>
        /// 静态构造函数，注册配置
        /// </summary>
        static DatabaseConfig()
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = DatabaseConfigJsonContext.Default,
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            ConfigManager.RegisterConfig<DatabaseConfig>(options);
        }
        
        /// <summary>
        /// 重写Save方法添加验证逻辑
        /// </summary>
        public override void Save()
        {
            // 验证连接字符串
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new InvalidOperationException("数据库连接字符串不能为空");
            }
            
            // 验证最大连接数
            if (MaxConnections <= 0 || MaxConnections > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxConnections), "最大连接数必须在1-1000之间");
            }
            
            // 验证连接超时
            if (ConnectionTimeout <= 0 || ConnectionTimeout > 300)
            {
                throw new ArgumentOutOfRangeException(nameof(ConnectionTimeout), "连接超时必须在1-300秒之间");
            }
            
            // 调用基类的Save方法
            base.Save();
        }
    }
    
    #endregion
    
    #region 应用配置示例
    
    /// <summary>
    /// 应用配置的JSON序列化上下文（AOT兼容）
    /// </summary>
    [JsonSerializable(typeof(ApplicationConfig))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class ApplicationConfigJsonContext : JsonSerializerContext
    {
    }
    
    /// <summary>
    /// 应用程序配置类
    /// </summary>
    public class ApplicationConfig : Config<ApplicationConfig>
    {
        /// <summary>
        /// 应用程序名称
        /// </summary>
        public string ApplicationName { get; set; } = "我的应用";
        
        /// <summary>
        /// 应用程序版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";
        
        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public bool DebugMode { get; set; } = false;
        
        /// <summary>
        /// 日志级别
        /// </summary>
        public string LogLevel { get; set; } = "Information";
        
        /// <summary>
        /// 缓存超时时间（分钟）
        /// </summary>
        public int CacheTimeoutMinutes { get; set; } = 30;
        
        /// <summary>
        /// 启用的功能模块
        /// </summary>
        public List<string> EnabledFeatures { get; set; } = new List<string> { "Logging", "Caching" };
        
        /// <summary>
        /// 自定义设置字典
        /// </summary>
        public Dictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// 静态构造函数，注册配置
        /// </summary>
        static ApplicationConfig()
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = ApplicationConfigJsonContext.Default,
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            ConfigManager.RegisterConfig<ApplicationConfig>(options);
        }
    }
    
    #endregion
    
    #region 配置使用示例
    
    /// <summary>
    /// 配置使用示例类
    /// </summary>
    public class ConfigurationUsageExample
    {
        /// <summary>
        /// 基本配置使用示例
        /// </summary>
        public static void BasicUsageExample()
        {
            Console.WriteLine("=== 基本配置使用示例 ===");
            
            // 获取数据库配置
            var dbConfig = DatabaseConfig.Current;
            Console.WriteLine($"数据库连接字符串: {dbConfig.ConnectionString}");
            Console.WriteLine($"最大连接数: {dbConfig.MaxConnections}");
            Console.WriteLine($"连接模式: {dbConfig.Mode}");
            
            // 修改配置
            dbConfig.MaxConnections = 200;
            dbConfig.ConnectionTimeout = 60;
            
            // 保存配置
            dbConfig.Save();
            Console.WriteLine("数据库配置已保存");
            
            // 获取应用配置
            var appConfig = ApplicationConfig.Current;
            Console.WriteLine($"\n应用名称: {appConfig.ApplicationName}");
            Console.WriteLine($"版本: {appConfig.Version}");
            Console.WriteLine($"调试模式: {appConfig.DebugMode}");
            Console.WriteLine($"启用的功能: {string.Join(", ", appConfig.EnabledFeatures)}");
        }
        
        /// <summary>
        /// 配置变更监听示例
        /// </summary>
        public static void ConfigChangeListenerExample()
        {
            Console.WriteLine("\n=== 配置变更监听示例 ===");
            
            // 订阅数据库配置变更
            ConfigManager.SubscribeConfigChanged<DatabaseConfig>(newConfig =>
            {
                Console.WriteLine($"数据库配置已更新，新的最大连接数: {newConfig.MaxConnections}");
            });
            
            // 订阅应用配置变更（带新旧值对比）
            ConfigManager.SubscribeConfigChanged<ApplicationConfig>((oldConfig, newConfig) =>
            {
                if (oldConfig.DebugMode != newConfig.DebugMode)
                {
                    Console.WriteLine($"调试模式已变更: {oldConfig.DebugMode} → {newConfig.DebugMode}");
                }
            });
            
            // 订阅所有配置变更
            ConfigManager.SubscribeAllConfigChanged(e =>
            {
                Console.WriteLine($"配置 {e.ConfigName} 在 {DateTime.Now:HH:mm:ss} 发生变更");
            });
            
            Console.WriteLine("配置变更监听器已设置，现在修改配置文件将触发事件");
        }
        
        /// <summary>
        /// 高级配置使用示例
        /// </summary>
        public static void AdvancedUsageExample()
        {
            Console.WriteLine("\n=== 高级配置使用示例 ===");
            
            try
            {
                // 获取应用配置并添加自定义设置
                var appConfig = ApplicationConfig.Current;
                appConfig.CustomSettings["ApiKey"] = "your-api-key-here";
                appConfig.CustomSettings["MaxRetries"] = "3";
                appConfig.EnabledFeatures.Add("Analytics");
                
                // 保存配置
                appConfig.Save();
                Console.WriteLine("应用配置已更新");
                
                // 手动重新加载配置
                ApplicationConfig.Reload();
                var reloadedConfig = ApplicationConfig.Current;
                Console.WriteLine($"重新加载后的自定义设置数量: {reloadedConfig.CustomSettings.Count}");
                
                // 禁用自动重新加载（在生产环境中可能需要）
                ConfigManager.SetAutoReload(false);
                Console.WriteLine("自动重新加载已禁用");
                
                // 重新启用自动重新加载
                ConfigManager.SetAutoReload(true);
                Console.WriteLine("自动重新加载已重新启用");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 配置验证示例
        /// </summary>
        public static void ValidationExample()
        {
            Console.WriteLine("\n=== 配置验证示例 ===");
            
            try
            {
                var dbConfig = DatabaseConfig.Current;
                
                // 尝试设置无效的最大连接数
                dbConfig.MaxConnections = -1;
                dbConfig.Save(); // 这将抛出异常
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"配置验证失败: {ex.Message}");
            }
            
            try
            {
                var dbConfig = DatabaseConfig.Current;
                
                // 尝试设置无效的连接字符串
                dbConfig.ConnectionString = "";
                dbConfig.Save(); // 这将抛出异常
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"配置验证失败: {ex.Message}");
            }
        }
    }
    
    #endregion
    
    #region 应用程序入口示例
    
    /// <summary>
    /// 示例程序入口
    /// </summary>
    public class ConfigurationExample
    {
        /// <summary>
        /// 运行所有配置示例
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("Pek.Common 配置系统示例程序");
            Console.WriteLine("=====================================");
            
            try
            {
                // 基本使用示例
                ConfigurationUsageExample.BasicUsageExample();
                
                // 配置变更监听示例
                ConfigurationUsageExample.ConfigChangeListenerExample();
                
                // 高级使用示例
                ConfigurationUsageExample.AdvancedUsageExample();
                
                // 配置验证示例
                ConfigurationUsageExample.ValidationExample();
                
                Console.WriteLine("\n所有示例运行完成！");
                Console.WriteLine("您可以尝试手动修改 Config 目录下的配置文件来测试自动重新加载功能。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"示例运行失败: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
            }
        }
        
        /// <summary>
        /// AOT环境下的配置初始化示例
        /// </summary>
        public static void InitializeForAot()
        {
            Console.WriteLine("正在进行AOT兼容性初始化...");
            
            try
            {
                // 手动触发静态构造函数
                var dbConfig = DatabaseConfig.Current;
                var appConfig = ApplicationConfig.Current;
                
                Console.WriteLine("AOT兼容性初始化完成");
                Console.WriteLine($"数据库配置已加载: {!string.IsNullOrEmpty(dbConfig.ConnectionString)}");
                Console.WriteLine($"应用配置已加载: {!string.IsNullOrEmpty(appConfig.ApplicationName)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AOT初始化失败: {ex.Message}");
                throw;
            }
        }
    }
    
    #endregion
}