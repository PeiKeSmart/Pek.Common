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
            // 方式1：使用新的配置变更详情事件（推荐）
            ConfigManager.SubscribeConfigChangeDetails<AppConfig>(details =>
            {
                Console.WriteLine($"🔔 [自定义处理] AppConfig 配置变更详情:");
                foreach (var change in details.PropertyChanges)
                {
                    Console.WriteLine($"  📝 {change.PropertyName}: {change.OldValue} → {change.NewValue}");
                    
                    // 根据具体属性变更执行相应逻辑
                    switch (change.PropertyName)
                    {
                        case nameof(AppConfig.ApiUrl):
                            Console.WriteLine("    🔄 检测到API地址变更，准备重新初始化HTTP客户端...");
                            break;
                        case nameof(AppConfig.MaxRetries):
                            Console.WriteLine("    🔄 检测到重试次数变更，准备更新重试策略...");
                            break;
                        case nameof(AppConfig.EnableLogging):
                            Console.WriteLine("    🔄 检测到日志开关变更，准备更新日志配置...");
                            break;
                    }
                }
                Console.WriteLine();
            });

            // 方式2：订阅数据库配置的详细变更
            ConfigManager.SubscribeConfigChangeDetails<DatabaseConfig>(details =>
            {
                Console.WriteLine($"🔔 [自定义处理] DatabaseConfig 配置变更详情:");
                foreach (var change in details.PropertyChanges)
                {
                    Console.WriteLine($"  📝 {change.PropertyName}: {change.OldValue} → {change.NewValue}");
                }
                
                // 数据库配置变更的业务逻辑处理
                if (details.PropertyChanges.Any(c => c.PropertyName == nameof(DatabaseConfig.ConnectionString)))
                {
                    Console.WriteLine("    🔄 连接字符串已变更，准备重新配置数据库连接池...");
                }
                Console.WriteLine();
            });

            // 方式3：全局配置变更监控（所有配置类型）
            ConfigManager.SubscribeAllConfigChangeDetails(details =>
            {
                Console.WriteLine($"📊 [全局监控] 配置 {details.ConfigName} 发生了 {details.PropertyChanges.Count} 个属性变更");
            });

            // 方式4：传统的新旧值比较方式（用于特殊逻辑）
            ConfigManager.SubscribeConfigChanged<AppConfig>((oldConfig, newConfig) =>
            {
                // 这里可以执行更复杂的业务逻辑
                if (oldConfig.ApiUrl != newConfig.ApiUrl)
                {
                    Console.WriteLine($"🔧 [业务逻辑] API基础地址变更，执行系统级重新配置...");
                    // 这里可以调用实际的业务逻辑
                    // HttpClientManager.ReconfigureBaseUrl(newConfig.ApiUrl);
                }
            });
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