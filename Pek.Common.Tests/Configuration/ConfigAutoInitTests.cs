using System;
using Xunit;
using Pek.Configuration;
using System.IO;

namespace Pek.Common.Tests.Configuration
{
    /// <summary>
    /// 配置自动初始化测试类
    /// </summary>
    public class ConfigAutoInitTests : IDisposable
    {
        // 构造函数用于测试初始化
        public ConfigAutoInitTests()
        {
            CleanupTestConfigs();
        }
        // 实现IDisposable用于测试清理
        public void Dispose()
        {
            CleanupTestConfigs();
        }
        
        /// <summary>
        /// 清理测试配置文件
        /// </summary>
        private void CleanupTestConfigs()
        {
            try
            {
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var configDir = Path.Combine(appDirectory, "Config");
                
                if (Directory.Exists(configDir))
                {
                    // 删除测试配置文件
                    var testConfigPath = Path.Combine(configDir, "TestConfig.config");
                    if (File.Exists(testConfigPath))
                    {
                        File.Delete(testConfigPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理测试环境失败: {ex.Message}");
            }
        }
        [Fact]
        public void Config_AutoInitialization_Works()
        {
            // 访问配置实例，应该触发自动初始化
            var settings = Settings.Current;
            
            // 验证配置实例不为空
            Assert.NotNull(settings);
            
            // 验证默认值已正确设置
            Assert.Equal("1.0.0", settings.Version);
            Assert.Equal(false, settings.Debug);
            Assert.Equal(30, settings.TimeoutSeconds);
        }
        
        [Fact]
        public void ConfigInitializer_ExplicitInitialization_Works()
        {
            // 显式初始化特定配置类
            ConfigInitializer.InitializeConfig<Settings>();
            
            // 验证配置实例不为空
            var settings = Settings.Current;
            Assert.NotNull(settings);
        }
        
        [Fact]
        public void ConfigInitializer_InitializeMultipleConfigs_Works()
        {
            // 初始化多个特定配置类
            ConfigInitializer.InitializeConfig<Settings>();
            ConfigInitializer.InitializeConfig<TestConfig>();
            
            // 验证配置实例不为空
            var settings = Settings.Current;
            var testConfig = TestConfig.Current;
            Assert.NotNull(settings);
            Assert.NotNull(testConfig);
        }
        
        [Fact]
        public void TestConfig_AutoInitialization_Works()
        {
            // 访问TestConfig实例，应该触发自动初始化
            var config = TestConfig.Current;
            
            // 验证配置实例不为空
            Assert.NotNull(config);
            
            // 验证默认值已正确设置
            Assert.Equal("TestConfig", config.Name);
            Assert.Equal(42, config.Value);
            Assert.Equal(true, config.Enabled);
            
            // 验证嵌套配置
            Assert.NotNull(config.Nested);
            Assert.Equal("SubConfig", config.Nested.SubName);
            Assert.Equal(100, config.Nested.SubValue);
        }
        
        [Fact]
        public void Config_SaveAndReload_Works()
        {
            // 获取配置实例
            var config = TestConfig.Current;
            
            // 修改配置值
            config.Name = "ModifiedName";
            config.Value = 99;
            config.Nested.SubName = "ModifiedSubName";
            
            // 保存配置
            config.Save();
            
            // 重新加载配置
            TestConfig.Reload();
            var reloadedConfig = TestConfig.Current;
            
            // 验证修改后的值已正确保存和加载
            Assert.Equal("ModifiedName", reloadedConfig.Name);
            Assert.Equal(99, reloadedConfig.Value);
            Assert.Equal("ModifiedSubName", reloadedConfig.Nested.SubName);
            
            // 清理：恢复默认值
            reloadedConfig.Name = "TestConfig";
            reloadedConfig.Value = 42;
            reloadedConfig.Nested.SubName = "SubConfig";
            reloadedConfig.Save();
        }
        
        [Fact]
        public void Config_FileCreation_Works()
        {
            // 获取应用程序根目录下的Config文件夹
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configDir = Path.Combine(appDirectory, "Config");
            var configFilePath = Path.Combine(configDir, "TestConfig.config");
            
            // 访问配置实例，应该触发自动初始化和文件创建
            var config = TestConfig.Current;
            config.Save(); // 确保文件被创建
            
            // 验证配置文件已创建
            Assert.True(File.Exists(configFilePath));
            
            // 验证文件内容
            var fileContent = File.ReadAllText(configFilePath);
            Assert.True(fileContent.Contains("TestConfig"));
            Assert.True(fileContent.Contains("42"));
            Assert.True(fileContent.Contains("SubConfig"));
        }
        
        [Fact]
        public void Config_AutoInitialization_Mechanism_Works()
        {
            // 创建一个新的配置类型的实例，但不通过Current属性访问
            // 这样不会触发自动初始化
            var directInstance = new TestConfig();
            
            // 修改实例的值
            directInstance.Name = "DirectInstance";
            directInstance.Value = 123;
            
            // 保存这个实例
            directInstance.Save();
            
            // 现在通过Current属性访问，这应该触发自动初始化
            // 并加载我们刚才保存的值
            var currentInstance = TestConfig.Current;
            
            // 验证值已正确加载
            Assert.Equal("DirectInstance", currentInstance.Name);
            Assert.Equal(123, currentInstance.Value);
            
            // 修改回默认值
            currentInstance.Name = "TestConfig";
            currentInstance.Value = 42;
            currentInstance.Save();
        }
        
        [Fact]
        public void Config_StaticConstructor_CalledOnlyOnce()
        {
            // 重置初始化计数器
            TestConfigWithCounter.InitCount = 0;
            
            // 多次访问Current属性
            var config1 = TestConfigWithCounter.Current;
            var config2 = TestConfigWithCounter.Current;
            var config3 = TestConfigWithCounter.Current;
            
            // 验证静态构造函数只被调用一次
            Assert.Equal(1, TestConfigWithCounter.InitCount);
        }
    }
}