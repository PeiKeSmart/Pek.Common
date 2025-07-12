using System.ComponentModel;

using NewLife.Configuration;

namespace Pek.Configs;

/// <summary>系统通用配置</summary>
[DisplayName("系统通用配置")]
[Config("PekSys")]
public class PekSysSetting : Config<PekSysSetting>
{
    /// <summary>机器人错误码。设置后拦截各种爬虫并返回相应错误，如404/500，默认0不拦截</summary>
    [Description("机器人错误码。设置后拦截各种爬虫并返回相应错误，如404/500，默认0不拦截")]
    [Category("通用")]
    public Int32 RobotError { get; set; }

    /// <summary>
    /// 是否允许获取请求和响应内容
    /// </summary>
    [Description("是否允许获取请求和响应内容")]
    public Boolean AllowRequestParams { get; set; } = true;

    /// <summary>
    /// AllowRequestParams为true时，允许获取请求和响应内容时排除的Url关键词，多个以逗号分隔
    /// </summary>
    [Description("AllowRequestParams为true时，允许获取请求和响应内容时排除的Url关键词，多个以逗号分隔")]
    public String? ExcludeUrl { get; set; }

    /// <summary>
    /// AllowRequestParams为false时，允许获取请求和响应内容时的Url关键词，多个以逗号分隔
    /// </summary>
    [Description("AllowRequestParams为false时，允许获取请求和响应内容时的Url关键词，多个以逗号分隔")]
    public String RequestParamsUrl { get; set; } = String.Empty;

    /// <summary>
    /// 在客户浏览器地址栏中启用博客 RSS feeds 链接
    /// </summary>
    [Description("在客户浏览器地址栏中启用博客 RSS feeds 链接")]
    public Boolean ShowHeaderRssUrl { get; set; }

    /// <summary>
    /// 是否启用表单数据净化，默认启用
    /// </summary>
    [Description("是否启用表单数据净化，默认启用")]
    public Boolean AllowFormDataSanitize { get; set; }

    /// <summary>
    /// 雪花ID机器ID，-1表示不配置，使用默认的WorkerId。一般用于分布式集群，每个节点配置不同的WorkerId，主要用于没有使用Redis也没有使用星尘的场景。
    /// </summary>
    [Description("雪花ID机器ID，-1表示不配置，使用默认的WorkerId。一般用于分布式集群，每个节点配置不同的WorkerId，主要用于没有使用Redis也没有使用星尘的场景。")]
    public Int32 SnowflakeWorkerId { get; set; } = -1;
}
