using System.ComponentModel;

using NewLife.Configuration;

namespace Pek.Configs;

/// <summary>安全配置</summary>
[DisplayName("安全配置")]
//[XmlConfigFile("Config/Security.config", 10_000)]
[Config("Security")]
public class SecuritySetting : Config<SecuritySetting>
{
    /// <summary>过滤违禁词</summary>
    [Description("词过滤")]
    public String? FilterWord { get; set; }

    /// <summary>电话黑名单</summary>
    [Description("电话黑名单")]
    public String? PhoneBlacklist { get; set; }

    /// <summary>邮箱域名白名单</summary>
    [Description("邮箱域名白名单")]
    public String? EmailDomainWhiteList { get; set; }

    /// <summary>邮箱域名黑名单</summary>
    [Description("邮箱域名黑名单")]
    public String? EmailDomainBlockList { get; set; }

    /// <summary>百度密钥</summary>
    [Description("百度密钥")]
    public String? BaiduAK { get; set; }

    /// <summary>真实客户端IP转发标头</summary>
    [Description("真实客户端IP转发标头")]
    public String TrueClientIPHeader { get; set; } = "CF-Connecting-IP";

    /// <summary>是否允许IP直接访问</summary>
    [Description("是否允许IP直接访问")]
    public Boolean EnableIPDirect { get; set; } = true;

    /// <summary>允许跨设备使用Token</summary>
    [Description("允许跨设备使用Token")]
    public Boolean AllowCrossDevice { get; set; } = false;
}
