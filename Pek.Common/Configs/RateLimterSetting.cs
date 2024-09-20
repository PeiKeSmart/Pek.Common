using System.ComponentModel;

using NewLife.Configuration;

namespace Pek.Configs;

/// <summary>限流配置</summary>
[DisplayName("限流配置")]
//[XmlConfigFile("Config/DHUtil.config", 10_000)]
[Config("RateLimterSetting")]
public class RateLimterSetting : Config<RateLimterSetting>
{
    /// <summary>
    /// 是否允许限流
    /// </summary>
    [Description("是否允许限流")]
    public Boolean AllowRateLimter { get; set; }
}
