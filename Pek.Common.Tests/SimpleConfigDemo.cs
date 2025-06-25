using Pek.Configuration;
using System;
using System.IO;

namespace Pek.Common.Tests;

/// <summary>
/// 简单的配置演示程序
/// 演示配置文件的创建位置，不会自动清理文件
/// </summary>
class SimpleConfigDemo
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== 配置文件创建演示 ===");
        Console.WriteLine();
        
        // 显示当前工作目录
        Console.WriteLine($"当前工作目录: {Environment.CurrentDirectory}");
        Console.WriteLine($"应用程序基础目录: {AppDomain.CurrentDomain.BaseDirectory}");
        Console.WriteLine();
        
        try
        {
            // 获取Settings配置实例
            Console.WriteLine("正在获取Settings配置实例...");
            var settings = Settings.Current;
            
            // 修改配置值
            settings.Name = "配置演示应用";
            settings.Version = "1.0.0";
            settings.Debug = true;
            settings.TimeoutSeconds = 30;
            
            Console.WriteLine("配置内容:");
            Console.WriteLine($"  Name: {settings.Name}");
            Console.WriteLine($"  Version: {settings.Version}");
            Console.WriteLine($"  Debug: {settings.Debug}");
            Console.WriteLine($"  TimeoutSeconds: {settings.TimeoutSeconds}");
            Console.WriteLine();
            
            // 保存配置
            Console.WriteLine("正在保存配置...");
            settings.Save();
            
            // 显示配置文件位置
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var configDir = Path.Combine(baseDir, "Config");
            var configFile = Path.Combine(configDir, "Settings.config");
            
            Console.WriteLine($"配置目录: {configDir}");
            Console.WriteLine($"配置文件: {configFile}");
            
            if (Directory.Exists(configDir))
            {
                Console.WriteLine("✅ Config目录已创建");
                
                if (File.Exists(configFile))
                {
                    Console.WriteLine("✅ Settings.config文件已创建");
                    Console.WriteLine($"文件大小: {new FileInfo(configFile).Length} 字节");
                    
                    Console.WriteLine();
                    Console.WriteLine("配置文件内容:");
                    Console.WriteLine(new string('-', 50));
                    Console.WriteLine(File.ReadAllText(configFile));
                    Console.WriteLine(new string('-', 50));
                }
                else
                {
                    Console.WriteLine("❌ Settings.config文件未找到");
                }
            }
            else
            {
                Console.WriteLine("❌ Config目录未找到");
            }
            
            Console.WriteLine();
            Console.WriteLine("✅ 演示完成！配置文件已保存，您可以在上述路径中查看。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 发生错误: {ex.Message}");
            Console.WriteLine($"详细信息: {ex}");
        }
        
        Console.WriteLine();
        Console.WriteLine("注意：此演示程序不会自动删除配置文件，");
        Console.WriteLine("您可以在应用程序输出目录的Config文件夹中找到生成的配置文件。");
    }
}