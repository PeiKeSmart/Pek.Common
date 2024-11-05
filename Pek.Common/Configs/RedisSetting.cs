using System.ComponentModel;

using NewLife.Configuration;

namespace Pek.Configs;

/// <summary>Redis配置</summary>
[DisplayName("Redis配置")]
//[XmlConfigFile("Config/DHUtil.config", 10_000)]
[Config("Redis")]
public class RedisSetting : Config<RedisSetting>
{
    /// <summary>
    /// Cache键前缀
    /// </summary>
    [Description("Cache键前缀")]
    public String CacheKeyPrefix { get; set; } = "";

    /// <summary>
    /// 是否应该使用Redis服务
    /// </summary>
    [Description("是否应该使用Redis服务")]
    public Boolean RedisEnabled { get; set; }
}
