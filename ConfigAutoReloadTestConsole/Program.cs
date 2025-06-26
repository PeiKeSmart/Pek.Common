using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Pek.Configuration;

namespace ConfigAutoReloadTestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== 通用配置变更事件测试（自动启用日志版）===");
            Console.WriteLine();

            // 不再需要手动启用默认的配置变更日志记录
            // ConfigManager.EnableDefaultChangeLogging(); // 系统会自动启用
            
            Console.WriteLine("✅ 配置系统已自动启用默认变更日志记录");

            // 设置自定义配置变更事件处理
            SetupCustomConfigEventHandlers();

            // 获取初始配置（这将触发ConfigManager的静态构造函数）
            var appConfig = AppConfig.Current;
            var dbConfig = DatabaseConfig.Current;
            
            Console.WriteLine("初始配置:");
            Console.WriteLine("=== AppConfig ===");
            PrintAppConfig(appConfig);
            Console.WriteLine();
            Console.WriteLine("=== DatabaseConfig ===");
            PrintDatabaseConfig(dbConfig);
            Console.WriteLine();

            // 显示配置文件路径
            Console.WriteLine("配置文件路径:");
            Console.WriteLine($"AppConfig: {GetConfigFilePath("AppConfig")}");
            Console.WriteLine($"DatabaseConfig: {GetConfigFilePath("DatabaseConfig")}");
            Console.WriteLine();

            Console.WriteLine("功能测试选项：");
            Console.WriteLine("1. 按 '1' - 代码修改应用配置");
            Console.WriteLine("2. 按 '2' - 代码修改数据库配置");
            Console.WriteLine("3. 按 '3' - 手动修改配置文件提示");
            Console.WriteLine("4. 按任意其他键 - 显示当前所有配置");
            Console.WriteLine("5. 按 'q' - 退出程序");
            Console.WriteLine();
            Console.WriteLine("🔥 系统已自动启用配置变更日志，无需手动配置！");
            Console.WriteLine();

            while (true)
            {
                var key = Console.ReadKey(true);
                
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    break;
                }
                else if (key.KeyChar == '1')
                {
                    await TestAppConfigSave();
                }
                else if (key.KeyChar == '2')
                {
                    await TestDatabaseConfigSave();
                }
                else if (key.KeyChar == '3')
                {
                    ShowManualEditInstructions();
                }
                else
                {
                    ShowCurrentConfigs();
                }
            }

            Console.WriteLine("程序已退出。");
        }

        /// <summary>
        /// 设置自定义配置变更事件处理程序
        /// </summary>
        private static void SetupCustomConfigEventHandlers()
        {
            // 方式1：订阅应用配置变更（类型安全）
            ConfigManager.AnyConfigChanged += (sender, e) =>
            {
                if (e.ConfigType == typeof(AppConfig) && e.NewConfig is AppConfig newConfig)
                {
                    Console.WriteLine($"🔔 [自定义处理] AppConfig 配置已变更:");
                    Console.WriteLine($"  📝 ApiUrl: {newConfig.ApiUrl}");
                    Console.WriteLine($"  📝 MaxRetries: {newConfig.MaxRetries}");
                    Console.WriteLine($"  📝 EnableLogging: {newConfig.EnableLogging}");
                    Console.WriteLine("    🔄 检测到应用配置变更，准备重新初始化相关组件...");
                    Console.WriteLine();
                }
            };

            // 方式2：订阅数据库配置变更（带新旧值比较）
            ConfigManager.AnyConfigChanged += (sender, e) =>
            {
                if (e.ConfigType == typeof(DatabaseConfig) && 
                    e.OldConfig is DatabaseConfig oldConfig && 
                    e.NewConfig is DatabaseConfig newConfig)
                {
                    Console.WriteLine($"🔔 [自定义处理] DatabaseConfig 配置已变更:");
                    
                    if (oldConfig.ConnectionString != newConfig.ConnectionString)
                    {
                        Console.WriteLine($"  📝 ConnectionString: {oldConfig.ConnectionString} → {newConfig.ConnectionString}");
                        Console.WriteLine("    🔄 连接字符串已变更，准备重新配置数据库连接池...");
                    }
                    
                    if (oldConfig.MaxConnections != newConfig.MaxConnections)
                    {
                        Console.WriteLine($"  📝 MaxConnections: {oldConfig.MaxConnections} → {newConfig.MaxConnections}");
                        Console.WriteLine("    🔄 最大连接数已变更，准备调整连接池大小...");
                    }
                    
                    if (oldConfig.EnableSqlLogging != newConfig.EnableSqlLogging)
                    {
                        Console.WriteLine($"  📝 EnableSqlLogging: {oldConfig.EnableSqlLogging} → {newConfig.EnableSqlLogging}");
                        Console.WriteLine("    🔄 SQL日志开关已变更，准备更新日志配置...");
                    }
                    
                    Console.WriteLine();
                }
            };

            // 方式3：全局配置变更监控（所有配置类型）
            ConfigManager.AnyConfigChanged += (sender, e) =>
            {
                Console.WriteLine($"📊 [全局监控] 配置 {e.ConfigName} 在 {DateTime.Now:HH:mm:ss} 发生变更");
            };

            // 方式4：特定业务逻辑处理
            ConfigManager.AnyConfigChanged += (sender, e) =>
            {
                if (e.ConfigType == typeof(AppConfig) && 
                    e.OldConfig is AppConfig oldAppConfig && 
                    e.NewConfig is AppConfig newAppConfig)
                {
                    // API地址变更的特定处理
                    if (oldAppConfig.ApiUrl != newAppConfig.ApiUrl)
                    {
                        Console.WriteLine($"🔧 [业务逻辑] API基础地址变更，执行系统级重新配置...");
                        // 这里可以调用实际的业务逻辑
                        // HttpClientManager.ReconfigureBaseUrl(newAppConfig.ApiUrl);
                    }
                }
            };
        }

        private static async Task TestAppConfigSave()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔧 执行代码修改应用配置测试...");
            
            var config = AppConfig.Current;
            
            // 修改多个属性
            config.ApiUrl = $"https://api-{DateTime.Now:HHmmss}.example.com";
            config.MaxRetries = new Random().Next(1, 10);
            config.EnableLogging = !config.EnableLogging;
            
            // 保存配置（这应该不会触发自动重新加载事件）
            config.Save();
            
            Console.WriteLine("✅ 应用配置保存完成，观察是否触发自动重新加载事件...");
            await Task.Delay(1000);
        }

        private static async Task TestDatabaseConfigSave()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔧 执行代码修改数据库配置测试...");
            
            var config = DatabaseConfig.Current;
            
            // 修改多个属性
            config.ConnectionString = $"Server=localhost;Database=Test_{DateTime.Now:HHmmss};";
            config.MaxConnections = new Random().Next(10, 100);
            config.EnableSqlLogging = !config.EnableSqlLogging;
            
            // 保存配置
            config.Save();
            
            Console.WriteLine("✅ 数据库配置保存完成");
            await Task.Delay(1000);
        }

        private static void ShowManualEditInstructions()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📝 手动修改配置文件测试：");
            Console.WriteLine("请选择以下任一配置文件进行手动修改：");
            Console.WriteLine($"1. AppConfig: {GetConfigFilePath("AppConfig")}");
            Console.WriteLine($"2. DatabaseConfig: {GetConfigFilePath("DatabaseConfig")}");
            Console.WriteLine();
            Console.WriteLine("修改步骤：");
            Console.WriteLine("1. 用文本编辑器打开配置文件");
            Console.WriteLine("2. 修改其中任意值（建议修改多个属性）");
            Console.WriteLine("3. 保存文件");
            Console.WriteLine("4. 观察控制台输出，系统会自动检测并详细记录所有变更");
            Console.WriteLine();
            Console.WriteLine("💡 提示：系统会自动识别哪些属性发生了变更，无需手动对比！");
            Console.WriteLine();
        }

        private static void ShowCurrentConfigs()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 当前所有配置:");
            Console.WriteLine("=== AppConfig ===");
            PrintAppConfig(AppConfig.Current);
            Console.WriteLine();
            Console.WriteLine("=== DatabaseConfig ===");
            PrintDatabaseConfig(DatabaseConfig.Current);
            Console.WriteLine();
        }

        private static void PrintAppConfig(AppConfig config)
        {
            Console.WriteLine($"  ApiUrl: {config.ApiUrl}");
            Console.WriteLine($"  MaxRetries: {config.MaxRetries}");
            Console.WriteLine($"  EnableLogging: {config.EnableLogging}");
        }

        private static void PrintDatabaseConfig(DatabaseConfig config)
        {
            Console.WriteLine($"  ConnectionString: {config.ConnectionString}");
            Console.WriteLine($"  MaxConnections: {config.MaxConnections}");
            Console.WriteLine($"  EnableSqlLogging: {config.EnableSqlLogging}");
        }

        private static string GetConfigFilePath(string configName)
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configDir = Path.Combine(appDirectory, "Config");
            return Path.Combine(configDir, $"{configName}.config");
        }
    }

    /// <summary>
    /// 应用配置类
    /// </summary>
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

    /// <summary>
    /// 数据库配置类
    /// </summary>
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

    /// <summary>
    /// AOT兼容的JSON序列化上下文
    /// </summary>
    [JsonSerializable(typeof(AppConfig))]
    public partial class AppConfigJsonContext : JsonSerializerContext
    {
    }

    /// <summary>
    /// AOT兼容的JSON序列化上下文
    /// </summary>
    [JsonSerializable(typeof(DatabaseConfig))]
    public partial class DatabaseConfigJsonContext : JsonSerializerContext
    {
    }
}