using System.ComponentModel;

using NewLife.Configuration;

namespace Pek.Configs;

/// <summary>系统通用配置</summary>
[DisplayName("系统通用配置")]
[Config("PekSys")]
public class PekSysSetting : Config<PekSysSetting>
{
    /// <summary>
    /// 系统初始化控制参数
    /// </summary>
    [Description("系统初始化控制参数,系统是否安装,true：已安装，false：未安装")]
    public Boolean IsInstalled { get; set; }

    /// <summary>
    /// 多语言类型
    /// </summary>
    [Description("多语言类型,0：默认当链接没有包含语言标识符时使用缓存，1：当链接没有包含语言标识符时使用默认语言")]
    public Int32 LanguageType { get; set; } = 0;

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

    /// <summary>是否启用维护模式</summary>
    [Description("维护模式。启用后系统将拒绝所有访问并显示维护页面")]
    [Category("系统维护")]
    public Boolean MaintenanceMode { get; set; } = false;

    /// <summary>维护模式提示信息</summary>
    [Description("维护模式页面显示的提示信息")]
    [Category("系统维护")]
    public String MaintenanceMessage { get; set; } = "系统正在进行维护升级，请稍后访问。给您带来不便，敬请谅解！";

    /// <summary>维护模式允许访问的IP地址</summary>
    [Description("维护模式下允许访问的IP地址列表，多个IP用逗号分隔")]
    [Category("系统维护")]
    public String MaintenanceAllowedIPs { get; set; } = "";

    /// <summary>维护模式开始时间</summary>
    [Description("维护模式开始的时间，用于计算用户剩余操作时间")]
    [Category("系统维护")]
    public DateTime MaintenanceStartTime { get; set; }

    /// <summary>已存在会话的冗余时间（分钟）</summary>
    [Description("启用维护模式时，给予已存在会话的用户继续操作的时间（分钟），0表示立即生效")]
    [Category("系统维护")]
    public Int32 MaintenanceGracePeriodMinutes { get; set; } = 15;

    /// <summary>维护模式下是否允许新会话</summary>
    [Description("维护模式下是否允许创建新会话，false表示拒绝所有新访问")]
    [Category("系统维护")]
    public Boolean MaintenanceAllowNewSessions { get; set; } = false;

    /// <summary>维护模式警告提前时间（分钟）</summary>
    [Description("在维护模式生效前多少分钟开始显示警告信息，0表示不显示警告")]
    [Category("系统维护")]
    public Int32 MaintenanceWarningMinutes { get; set; } = 5;

    #region 短码生成配置

    /// <summary>短码计数器当前值</summary>
    [Description("短码生成器的当前计数器值，用于Redis故障时的恢复")]
    [Category("短码生成")]
    public Int64 ShortCodeCounter { get; set; } = 238328;

    /// <summary>短码计数器最后更新时间</summary>
    [Description("短码计数器最后一次更新的时间")]
    [Category("短码生成")]
    public DateTime ShortCodeCounterLastUpdate { get; set; } = DateTime.MinValue;

    /// <summary>短码计数器备份间隔（个数）</summary>
    [Description("每生成多少个短码后备份一次计数器到配置，0表示每次都备份")]
    [Category("短码生成")]
    public Int32 ShortCodeBackupInterval { get; set; } = 100;

    #endregion

    /// <summary>本地调试代理地址。主要用于访问线上服务器内部服务</summary>
    [Description("本地调试代理地址。主要用于访问线上服务器内部服务")]
    public String LocalProxyUrl { get; set; } = "https://proxy.0ht.cn";

    /// <summary>本地调试l连接代理地址时需要的码</summary>
    [Description("本地调试连接代理地址时需要的码")]
    public String LocalProxyCode { get; set; } = "";

    /// <summary>临时允许跨设备使用Token</summary>
    [Description("临时允许跨设备使用Token")]
    public Boolean AllowJwtCrossDevice { get; set; } = false;

    /// <summary>允许前台封装的控制器同时支持PC和H5的视图，用逗号分隔。如Login</summary>
    [Description("允许前台封装的控制器同时支持PC和H5的视图，用逗号分隔。如Login")]
    public String EnableFrontMobile { get; set; } = String.Empty;

    /// <summary>允许后台封装的控制器同时支持PC和H5的视图，用逗号分隔。如Login</summary>
    [Description("允许后台封装的控制器同时支持PC和H5的视图，用逗号分隔。如Login")]
    public String EnableBackendMobile { get; set; } = String.Empty;
}
