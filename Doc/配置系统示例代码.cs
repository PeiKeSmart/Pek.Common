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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            
            ConfigManager.RegisterConfig<DatabaseConfig>(options, "DatabaseConfig");
        }
    }
    
    #endregion
    
    #region 应用程序示例
    
    /// <summary>
    /// 示例程序
    /// </summary>
    public class ConfigurationExample
    {
        /// <summary>
        /// 运行示例
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("=== 配置系统示例 ===\n");
            
            // 初始化配置
            InitializeConfigs();
            
            // 读取配置
            ReadConfig();
            
            // 修改配置
            ModifyConfig();
            
            // 重新加载配置
            ReloadConfig();
            
            Console.WriteLine("\n=== 示例结束 ===");
        }
        
        /// <summary>
        /// 初始化配置
        /// </summary>
        private static void InitializeConfigs()
        {
            Console.WriteLine("初始化配置...");
            
            try
            {
                // 预先初始化DatabaseConfig
                var jsonOptions = new JsonSerializerOptions
                {
                    TypeInfoResolver = DatabaseConfigJsonContext.Default,
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                
                // 手动注册配置
                ConfigManager.RegisterConfig<DatabaseConfig>(jsonOptions, "DatabaseConfig");
                
                // 访问配置触发初始化
                var config = DatabaseConfig.Current;
                Console.WriteLine("✅ 配置初始化成功\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ 配置初始化失败: {ex.Message}\n");
            }
        }
        
        /// <summary>
        /// 读取配置
        /// </summary>
        private static void ReadConfig()
        {
            Console.WriteLine("读取配置...");
            
            var config = DatabaseConfig.Current;
            
            Console.WriteLine($"连接字符串: {config.ConnectionString}");
            Console.WriteLine($"最大连接数: {config.MaxConnections}");
            Console.WriteLine($"连接超时: {config.ConnectionTimeout}秒");
            Console.WriteLine($"启用连接池: {config.EnableConnectionPooling}");
            Console.WriteLine($"连接模式: {config.Mode}");
            Console.WriteLine("允许的IP地址:");
            foreach (var ip in config.AllowedIpAddresses)
            {
                Console.WriteLine($"  - {ip}");
            }
            
            Console.WriteLine("✅ 配置读取成功\n");
        }
        
        /// <summary>
        /// 修改配置
        /// </summary>
        private static void ModifyConfig()
        {
            Console.WriteLine("修改配置...");
            
            var config = DatabaseConfig.Current;
            
            // 修改配置值
            config.ConnectionString = "Server=db.example.com;Database=ProductionDb;User Id=admin;Password=secure123;";
            config.MaxConnections = 200;
            config.ConnectionTimeout = 60;
            config.Mode = ConnectionMode.HighReliability;
            config.AllowedIpAddresses.Add("192.168.1.100");
            
            // 保存配置
            config.Save();
            
            Console.WriteLine("修改后的配置:");
            Console.WriteLine($"连接字符串: {config.ConnectionString}");
            Console.WriteLine($"最大连接数: {config.MaxConnections}");
            Console.WriteLine($"连接超时: {config.ConnectionTimeout}秒");
            Console.WriteLine($"连接模式: {config.Mode}");
            Console.WriteLine("允许的IP地址:");
            foreach (var ip in config.AllowedIpAddresses)
            {
                Console.WriteLine($"  - {ip}");
            }
            
            Console.WriteLine("✅ 配置修改并保存成功\n");
        }
        
        /// <summary>
        /// 重新加载配置
        /// </summary>
        private static void ReloadConfig()
        {
            Console.WriteLine("重新加载配置...");
            
            // 重新加载配置
            DatabaseConfig.Reload();
            
            var config = DatabaseConfig.Current;
            
            Console.WriteLine("重新加载后的配置:");
            Console.WriteLine($"连接字符串: {config.ConnectionString}");
            Console.WriteLine($"最大连接数: {config.MaxConnections}");
            Console.WriteLine($"连接超时: {config.ConnectionTimeout}秒");
            
            Console.WriteLine("✅ 配置重新加载成功\n");
        }
    }
    
    #endregion
}