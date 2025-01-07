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
    /// 是否应该使用Redis服务
    /// </summary>
    [Description("是否应该使用Redis服务")]
    public Boolean RedisEnabled { get; set; }

    /// <summary>
    /// Cache键前缀
    /// </summary>
    [Description("Cache键前缀")]
    public String CacheKeyPrefix { get; set; } = "";

    /// <summary>
    /// 获取或设置Redis连接字符串。 启用Redis时使用
    /// </summary>
    [Description("Redis连接字符串")]
    public String RedisConnectionString { get; set; } = "127.0.0.1:6379";

    /// <summary>
    /// 获取或设置Redis连接密码
    /// </summary>
    [Description("Redis连接密码")]
    public String? RedisPassWord { get; set; }

    /// <summary>
    /// 获取或设置特定的Redis数据库； 如果需要使用特定的Redis数据库，只需在此处设置其编号。 如果应该为每种数据类型使用不同的数据库，则设置NULL（默认使用）
    /// </summary>
    [Description("特定的Redis数据库")]
    public Int32 RedisDatabaseId { get; set; } = 2;
}
