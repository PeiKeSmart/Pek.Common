using System.Text.Json.Serialization;
using Pek.Configuration;

namespace Pek.Common.Tests.Configuration
{
    /// <summary>
    /// 测试配置类
    /// </summary>
    public class TestConfig : Config<TestConfig>
    {
        /// <summary>
        /// 测试名称
        /// </summary>
        public string Name { get; set; } = "TestConfig";
        
        /// <summary>
        /// 测试值
        /// </summary>
        public int Value { get; set; } = 42;
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// 嵌套配置
        /// </summary>
        public NestedConfig Nested { get; set; } = new NestedConfig();
        
        /// <summary>
        /// 静态构造函数，注册配置
        /// </summary>
        static TestConfig()
        {
            RegisterConfigForAot(TestConfigJsonContext.Default);
        }
    }
    
    /// <summary>
    /// 嵌套配置类
    /// </summary>
    public class NestedConfig
    {
        /// <summary>
        /// 嵌套名称
        /// </summary>
        public string SubName { get; set; } = "SubConfig";
        
        /// <summary>
        /// 嵌套值
        /// </summary>
        public int SubValue { get; set; } = 100;
    }
    
    /// <summary>
    /// AOT兼容的JSON序列化上下文
    /// </summary>
    [JsonSerializable(typeof(TestConfig))]
    [JsonSerializable(typeof(NestedConfig))]
    public partial class TestConfigJsonContext : ConfigJsonContext
    {
    }
}