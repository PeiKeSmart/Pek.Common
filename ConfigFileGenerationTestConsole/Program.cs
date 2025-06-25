using System;
using System.IO;
using Pek.Configuration;

namespace ConfigFileGenerationTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== 配置文件生成测试 ===");
            Console.WriteLine();

            // 显示当前工作目录
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configDir = Path.Combine(appDirectory, "Config");
            var configFilePath = Path.Combine(configDir, "Settings.config");

            Console.WriteLine($"应用程序目录: {appDirectory}");
            Console.WriteLine($"配置文件目录: {configDir}");
            Console.WriteLine($"配置文件路径: {configFilePath}");
            Console.WriteLine();

            // 检查配置文件是否已存在
            Console.WriteLine("=== 检查配置文件状态 ===");
            Console.WriteLine($"Config目录是否存在: {Directory.Exists(configDir)}");
            Console.WriteLine($"配置文件是否存在: {File.Exists(configFilePath)}");
            Console.WriteLine();

            // 第一次访问配置（应该创建默认配置）
            Console.WriteLine("=== 第一次访问配置 ===");
            var config = TestSettings.Current;
            Console.WriteLine($"配置名称: {config.Name ?? "null"}");
            Console.WriteLine($"配置版本: {config.Version}");
            Console.WriteLine($"调试模式: {config.Debug}");
            Console.WriteLine($"超时时间: {config.TimeoutSeconds}秒");
            Console.WriteLine();

            // 检查配置文件是否被创建
            Console.WriteLine("=== 检查配置文件是否被创建 ===");
            Console.WriteLine($"Config目录是否存在: {Directory.Exists(configDir)}");
            Console.WriteLine($"配置文件是否存在: {File.Exists(configFilePath)}");
            if (File.Exists(configFilePath))
            {
                Console.WriteLine("配置文件内容:");
                Console.WriteLine(File.ReadAllText(configFilePath));
            }
            Console.WriteLine();

            // 修改配置并保存
            Console.WriteLine("=== 修改配置并保存 ===");
            config.Name = "测试应用配置";
            config.Version = "2.1.0";
            config.Debug = true;
            config.TimeoutSeconds = 90;

            Console.WriteLine("修改后的配置:");
            Console.WriteLine($"配置名称: {config.Name}");
            Console.WriteLine($"配置版本: {config.Version}");
            Console.WriteLine($"调试模式: {config.Debug}");
            Console.WriteLine($"超时时间: {config.TimeoutSeconds}秒");

            // 保存配置
            config.Save();
            Console.WriteLine("配置已保存!");
            Console.WriteLine();

            // 验证保存后的文件内容
            Console.WriteLine("=== 验证保存后的配置文件 ===");
            if (File.Exists(configFilePath))
            {
                Console.WriteLine("保存后的配置文件内容:");
                Console.WriteLine(File.ReadAllText(configFilePath));
            }
            else
            {
                Console.WriteLine("错误: 配置文件未找到!");
            }
            Console.WriteLine();

            // 测试重新加载
            Console.WriteLine("=== 测试配置重新加载 ===");
            TestSettings.Reload();
            var reloadedConfig = TestSettings.Current;
            Console.WriteLine("重新加载后的配置:");
            Console.WriteLine($"配置名称: {reloadedConfig.Name}");
            Console.WriteLine($"配置版本: {reloadedConfig.Version}");
            Console.WriteLine($"调试模式: {reloadedConfig.Debug}");
            Console.WriteLine($"超时时间: {reloadedConfig.TimeoutSeconds}秒");
            Console.WriteLine();

            Console.WriteLine("=== 测试完成 ===");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}