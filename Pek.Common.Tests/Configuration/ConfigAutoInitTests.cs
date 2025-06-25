using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pek.Configuration;
using System.IO;

namespace Pek.Common.Tests.Configuration
{
    /// <summary>
    /// 配置自动初始化测试类
    /// </summary>
    [TestClass]
    public class ConfigAutoInitTests
    {
        /// <summary>
        /// 测试初始化
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            // 清理测试环境
            CleanupTestConfigs();
        }
        
        /// <summary>
        /// 测试清理
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // 清理测试环境
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
        [TestMethod]
        public void Config_AutoInitialization_Works()
        {
            // 访问配置实例，应该触发自动初始化
            var settings = Settings.Current;
            
            // 验证配置实例不为空
            Assert.IsNotNull(settings);
            
            // 验证默认值已正确设置
            Assert.AreEqual("1.0.0", settings.Version);
            Assert.AreEqual(false, settings.Debug);
            Assert.AreEqual(30, settings.TimeoutSeconds);
        }
        
        [TestMethod]
        public void ConfigInitializer_ExplicitInitialization_Works()
        {
            // 显式初始化特定配置类
            ConfigInitializer.InitializeConfig<Settings>();
            
            // 验证配置实例不为空
            var settings = Settings.Current;
            Assert.IsNotNull(settings);
        }
        
        [TestMethod]
        public void ConfigInitializer_InitializeMultipleConfigs_Works()
        {
            // 初始化多个特定配置类
            ConfigInitializer.InitializeConfig<Settings>();
            ConfigInitializer.InitializeConfig<TestConfig>();
            
            // 验证配置实例不为空
            var settings = Settings.Current;
            var testConfig = TestConfig.Current;
            Assert.IsNotNull(settings);
            Assert.IsNotNull(testConfig);
        }
        
        [TestMethod]
        public void TestConfig_AutoInitialization_Works()
        {
            // 访问TestConfig实例，应该触发自动初始化
            var config = TestConfig.Current;
            
            // 验证配置实例不为空
            Assert.IsNotNull(config);
            
            // 验证默认值已正确设置
            Assert.AreEqual("TestConfig", config.Name);
            Assert.AreEqual(42, config.Value);
            Assert.AreEqual(true, config.Enabled);
            
            // 验证嵌套配置
            Assert.IsNotNull(config.Nested);
            Assert.AreEqual("SubConfig", config.Nested.SubName);
            Assert.AreEqual(100, config.Nested.SubValue);
        }
        
        [TestMethod]
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
            Assert.AreEqual("ModifiedName", reloadedConfig.Name);
            Assert.AreEqual(99, reloadedConfig.Value);
            Assert.AreEqual("ModifiedSubName", reloadedConfig.Nested.SubName);
            
            // 清理：恢复默认值
            reloadedConfig.Name = "TestConfig";
            reloadedConfig.Value = 42;
            reloadedConfig.Nested.SubName = "SubConfig";
            reloadedConfig.Save();
        }
        
        [TestMethod]
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
            Assert.IsTrue(File.Exists(configFilePath));
            
            // 验证文件内容
            var fileContent = File.ReadAllText(configFilePath);
            Assert.IsTrue(fileContent.Contains("TestConfig"));
            Assert.IsTrue(fileContent.Contains("42"));
            Assert.IsTrue(fileContent.Contains("SubConfig"));
        }
        
        [TestMethod]
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
            Assert.AreEqual("DirectInstance", currentInstance.Name);
            Assert.AreEqual(123, currentInstance.Value);
            
            // 修改回默认值
            currentInstance.Name = "TestConfig";
            currentInstance.Value = 42;
            currentInstance.Save();
        }
        
        [TestMethod]
        public void Config_StaticConstructor_CalledOnlyOnce()
        {
            // 重置初始化计数器
            TestConfigWithCounter.InitCount = 0;
            
            // 多次访问Current属性
            var config1 = TestConfigWithCounter.Current;
            var config2 = TestConfigWithCounter.Current;
            var config3 = TestConfigWithCounter.Current;
            
            // 验证静态构造函数只被调用一次
            Assert.AreEqual(1, TestConfigWithCounter.InitCount);
        }
    }
}