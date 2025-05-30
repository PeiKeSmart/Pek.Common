using System.ComponentModel;

using NewLife.Configuration;

namespace Pek.Configs;

/// <summary>公共存储空间设置</summary>
[DisplayName("公共存储空间设置")]
[Config("OssSetting")]
public class OssSetting : Config<OssSetting>
{
    /// <summary>
    /// 文件存储方式，0为本地存储，1为阿里云OSS存储，2为七牛云存储，3为Ucloud存储
    /// </summary>
    [Description("文件存储方式，0为本地存储，1为阿里云OSS存储，2为七牛云存储，3为Ucloud存储")]
    public Int32 UploadType { get; set; } = 0;

    /// <summary>
    /// 七牛OSS配置
    /// </summary>
    [Description("七牛OSS配置")]
    public QiNiuSetting QiNiu { get; set; } = new QiNiuSetting();

    /// <summary>
    /// 阿里OSS配置
    /// </summary>
    [Description("阿里OSS配置")]
    public AliOSS AliOSS { get; set; } = new AliOSS();
}

/// <summary>私有存储空间设置</summary>
[DisplayName("私有存储空间设置")]
[Config("PrivateOssSetting")]
public class PrivateOssSetting : Config<PrivateOssSetting>
{
    /// <summary>
    /// 文件存储方式，0为本地存储，1为阿里云OSS存储，2为七牛云存储，3为Ucloud存储
    /// </summary>
    [Description("文件存储方式，0为本地存储，1为阿里云OSS存储，2为七牛云存储，3为Ucloud存储")]
    public Int32 UploadType { get; set; } = 0;

    /// <summary>
    /// 七牛OSS配置
    /// </summary>
    [Description("七牛OSS配置")]
    public QiNiuSetting QiNiu { get; set; } = new QiNiuSetting();

    /// <summary>
    /// 阿里OSS配置
    /// </summary>
    [Description("阿里OSS配置")]
    public AliOSS AliOSS { get; set; } = new AliOSS();
}

/// <summary>
/// 七牛云OSS配置
/// </summary>
[DisplayName("七牛云OSS配置")]
public class QiNiuSetting
{
    /// <summary>
    /// 七牛云是否CNAME指向域名
    /// </summary>
    [Description("七牛云是否CNAME指向域名")]
    public Boolean IsQiNiuEndPoint { get; set; }

    /// <summary>
    /// 授权密钥
    /// </summary>
    [Description("授权密钥")]
    public String? AccessKey { get; set; }

    /// <summary>
    /// 密钥
    /// </summary>
    [Description("密钥")]
    public String? SecretKey { get; set; }

    /// <summary>
    /// 存储空间块
    /// </summary>
    [Description("存储空间块")]
    public String? Bucket { get; set; }

    /// <summary>
    /// 存储区域
    /// </summary>
    [Description("存储区域：华东、华北、华南、北美及东南亚")]
    public String? Zone { get; set; }

    /// <summary>
    /// 基本路径
    /// </summary>
    [Description("基本路径")]
    public String? BasePath { get; set; }

    /// <summary>
    /// 绑定域名
    /// </summary>
    [Description("绑定域名")]
    public String? Domain { get; set; }
}

/// <summary>
/// 阿里OSS配置
/// </summary>
[DisplayName("阿里OSS配置")]
public class AliOSS
{
    /// <summary>
    /// 阿里云OssEndpoint是否CNAME指向域名
    /// </summary>
    [Description("阿里云OssEndpoint是否CNAME指向域名")]
    public Boolean IsAliEndPoint { get; set; }

    /// <summary>
    /// 阿里云OssAccessKeyId
    /// </summary>
    [Description("阿里云OssAccessKeyId")]
    public String? OssAccessKeyId { get; set; }

    /// <summary>
    /// 阿里云OssSecretAccess
    /// </summary>
    [Description("阿里云OssSecretAccess")]
    public String? OssSecretAccess { get; set; }

    /// <summary>
    /// 阿里云OssBucket
    /// </summary>
    [Description("阿里云OssBucket")]
    public String? OssBucket { get; set; }

    /// <summary>
    /// 阿里云OssEndpoint
    /// </summary>
    [Description("阿里云OssEndpoint")]
    public String? OssEndpoint { get; set; }
}