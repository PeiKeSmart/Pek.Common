using System.Text.Json.Serialization;
using Pek.Configuration;

namespace Pek.Common.Tests.Configuration
{
    /// <summary>
    /// 带初始化计数器的测试配置类
    /// </summary>
    public class TestConfigWithCounter : Config<TestConfigWithCounter>
    {
        /// <summary>
        /// 初始化计数器
        /// </summary>
        public static int InitCount { get; set; } = 0;
        
        /// <summary>
        /// 测试名称
        /// </summary>
        public string Name { get; set; } = "CounterConfig";
        
        /// <summary>
        /// 测试值
        /// </summary>
        public int Value { get; set; } = 42;
        
        /// <summary>
        /// 静态构造函数，注册配置并增加计数器
        /// </summary>
        static TestConfigWithCounter()
        {
            // 增加初始化计数
            InitCount++;
            
            // 注册配置
            RegisterConfigForAot(TestConfigWithCounterJsonContext.Default);
        }
    }
    
    /// <summary>
    /// AOT兼容的JSON序列化上下文
    /// </summary>
    [JsonSerializable(typeof(TestConfigWithCounter))]
    public partial class TestConfigWithCounterJsonContext : ConfigJsonContext
    {
    }
}