using Pek.Common.Tests.Configuration;
using Xunit.Abstractions;

namespace Pek.Common.Tests;

/// <summary>
/// 测试运行器
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Pek.Common 配置系统测试 ===");
        Console.WriteLine();
        
        // 创建测试输出助手
        var output = new ConsoleTestOutputHelper();
        
        try
        {
            // 运行配置测试
            RunConfigurationTests(output);
            
            Console.WriteLine();
            Console.WriteLine("✅ 所有测试通过!");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ 测试失败: {ex.Message}");
            Console.WriteLine($"详细信息: {ex}");
        }
        
        Console.WriteLine("=== 测试完成 ===");
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
    
    private static void RunConfigurationTests(ITestOutputHelper output)
    {
        Console.WriteLine("开始运行配置系统测试...");
        Console.WriteLine();
        
        // 确保Settings类已初始化（触发静态构造函数）
        // 这是AOT兼容的方式，不使用反射
        try
        {
            // 预先初始化Settings类，确保其静态构造函数被调用
            // 在AOT环境下，确保SettingsJsonContext已被初始化
            Pek.Configuration.Settings.RegisterForAot(Pek.Configuration.SettingsJsonContext.Default,
        "Settings");

            // 访问Settings.Current触发初始化
            var settings = Pek.Configuration.Settings.Current;
            Console.WriteLine("✅ Settings配置已预先初始化");
        }
        catch (Exception ex)
        {
            // 在AOT环境中，可能会因为反射限制而无法初始化
            Console.WriteLine($"⚠️ Settings配置初始化失败: {ex.Message}");
        }
        
        using var tests = new ConfigurationTests(output);
        
        // 运行所有测试方法
        tests.TestConfigSave();
        Console.WriteLine();
        
        tests.TestConfigReload();
        Console.WriteLine();
        
        tests.TestConfigFilePath();
        Console.WriteLine();
        
        tests.TestConfigPersistence();
        Console.WriteLine();
        
        tests.TestConfigDefaults();
        Console.WriteLine();
        
        tests.TestPerformance();
        Console.WriteLine();
        
        tests.TestConfigFileExtension();
        Console.WriteLine();
        
        tests.TestConfigDirectoryStructure();
    }
}

/// <summary>
/// 控制台测试输出助手
/// </summary>
public class ConsoleTestOutputHelper : ITestOutputHelper
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        Console.WriteLine(format, args);
    }
}