using Pek.Configuration;
using System;
using System.IO;

namespace Pek.Common.Tests;

/// <summary>
/// 配置系统演示程序
/// 用于展示配置文件的创建和保存位置
/// </summary>
public class ConfigDemo
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 配置系统演示 ===");
        Console.WriteLine();
        
        try
        {
            // 显示应用程序基础目录
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine($"应用程序基础目录: {baseDirectory}");
            
            // 显示配置文件将要保存的位置
            var configDir = Path.Combine(baseDirectory, "Config");
            var configFilePath = Path.Combine(configDir, "Settings.config");
            Console.WriteLine($"配置目录: {configDir}");
            Console.WriteLine($"配置文件路径: {configFilePath}");
            Console.WriteLine();
            
            // 获取配置实例（这会触发配置文件的创建）
            Console.WriteLine("正在获取配置实例...");
            var settings = Settings.Current;
            
            // 修改一些配置值
            settings.Name = "演示应用";
            settings.Version = "1.0.0";
            settings.Debug = true;
            settings.TimeoutSeconds = 45;
            
            Console.WriteLine($"配置内容:");
            Console.WriteLine($"  Name: {settings.Name}");
            Console.WriteLine($"  Version: {settings.Version}");
            Console.WriteLine($"  Debug: {settings.Debug}");
            Console.WriteLine($"  TimeoutSeconds: {settings.TimeoutSeconds}");
            Console.WriteLine();
            
            // 保存配置
            Console.WriteLine("正在保存配置...");
            settings.Save();
            
            // 检查文件是否创建
            if (File.Exists(configFilePath))
            {
                Console.WriteLine("✅ 配置文件创建成功!");
                Console.WriteLine($"文件大小: {new FileInfo(configFilePath).Length} 字节");
                
                // 显示文件内容
                Console.WriteLine();
                Console.WriteLine("配置文件内容:");
                Console.WriteLine("=" + new string('=', 50));
                Console.WriteLine(File.ReadAllText(configFilePath));
                Console.WriteLine("=" + new string('=', 50));
            }
            else
            {
                Console.WriteLine("❌ 配置文件未创建");
            }
            
            Console.WriteLine();
            Console.WriteLine("演示完成。配置文件已保存，不会被自动删除。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 发生错误: {ex.Message}");
            Console.WriteLine($"详细信息: {ex}");
        }
        
        Console.WriteLine();
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}