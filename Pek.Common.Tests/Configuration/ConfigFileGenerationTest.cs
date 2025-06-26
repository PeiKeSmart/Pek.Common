using System;
using System.IO;
using Pek.Configuration;
using Xunit;

namespace Pek.Common.Tests.Configuration
{
    /// <summary>
    /// 配置文件生成测试
    /// </summary>
    public class ConfigFileGenerationTest
    {
        [Fact]
        public void TestConfigFileGeneration()
        {
            // 获取应用程序根目录下的Config文件夹路径
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configDir = Path.Combine(appDirectory, "Config");
            var configFilePath = Path.Combine(configDir, "Settings.config");

            // 清理可能存在的配置文件
            if (File.Exists(configFilePath))
            {
                File.Delete(configFilePath);
            }
            if (Directory.Exists(configDir))
            {
                Directory.Delete(configDir, true);
            }

            // 访问配置实例，这应该触发配置文件的生成
            var config = TestSettings.Current;
            
            // 修改配置属性
            config.Name = "测试配置";
            config.Version = "2.0.0";
            config.Debug = true;
            config.TimeoutSeconds = 60;

            // 保存配置
            config.Save();

            // 验证配置文件是否生成
            Assert.True(Directory.Exists(configDir), "Config目录应该被创建");
            Assert.True(File.Exists(configFilePath), "配置文件应该被生成");

            // 验证配置文件内容
            var fileContent = File.ReadAllText(configFilePath);
            Assert.Contains("测试配置", fileContent);
            Assert.Contains("2.0.0", fileContent);
            Assert.Contains("true", fileContent);
            Assert.Contains("60", fileContent);

            Console.WriteLine($"配置文件已生成: {configFilePath}");
            Console.WriteLine($"配置文件内容: {fileContent}");
        }

        [Fact]
        public void TestConfigFileReload()
        {
            // 获取配置文件路径
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configDir = Path.Combine(appDirectory, "Config");
            var configFilePath = Path.Combine(configDir, "Settings.config");

            // 确保目录存在
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            // 手动创建一个配置文件
            var testJson = @"{
  ""name"": ""手动创建的配置"",
  ""version"": ""3.0.0"",
  ""debug"": false,
  ""timeoutSeconds"": 120
}";
            File.WriteAllText(configFilePath, testJson);

            // 重新加载配置
            TestSettings.Reload();
            var config = TestSettings.Current;

            // 验证配置是否正确加载
            Assert.Equal("手动创建的配置", config.Name);
            Assert.Equal("3.0.0", config.Version);
            Assert.False(config.Debug);
            Assert.Equal(120, config.TimeoutSeconds);

            Console.WriteLine($"配置重新加载成功: Name={config.Name}, Version={config.Version}");
        }

        [Fact]
        public void TestMultipleConfigInstances()
        {
            // 测试多次获取配置实例是否返回同一个对象
            var config1 = TestSettings.Current;
            var config2 = TestSettings.Current;

            Assert.Same(config1, config2);
            Console.WriteLine("多次获取配置实例返回同一个对象，符合单例模式");
        }
    }
}