using System.Text.Json.Serialization;
using Pek.Configuration;

namespace Pek.Common.Tests.Configuration
{
    /// <summary>
    /// 配置
    /// </summary>
    public class TestSettings : Config<TestSettings>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string? Version { get; set; } = "1.0.0";

        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 静态构造函数，注册配置
        /// </summary>
        static TestSettings()
        {
            // 方案1：简化的AOT注册（推荐）
            RegisterForAot<TestSettingsJsonContext>("TestSettings");
        }
    }

    /// <summary>
    /// TestSettings的AOT兼容JSON序列化上下文
    /// 继承ConfigJsonContext获得基本类型支持，只需添加TConfig类型
    /// </summary>
    [JsonSerializable(typeof(TestSettings))]
    public partial class TestSettingsJsonContext : ConfigJsonContext
    {
    }
}