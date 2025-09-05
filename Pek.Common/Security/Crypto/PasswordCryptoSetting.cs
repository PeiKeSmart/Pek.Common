using System.ComponentModel;

using NewLife.Configuration;

using Pek.Helpers;

namespace Pek.Security.Crypto;

[DisplayName("密码可逆加密配置")]
[Config("PasswordCrypto")]
public class PasswordCryptoSetting : Config<PasswordCryptoSetting>
{
    /// <summary>
    /// 主密钥池（支持密钥轮换）
    /// 最新的密钥在数组末尾，旧密钥保留用于解密历史数据
    /// </summary>
    [Description("主密钥池（支持密钥轮换）")]
    public String[] PasswordKeys { get; set; } = [Randoms.RandomStr(32), Randoms.RandomStr(32), Randoms.RandomStr(32), Randoms.RandomStr(32), Randoms.RandomStr(32), Randoms.RandomStr(32), Randoms.RandomStr(32), Randoms.RandomStr(32), Randoms.RandomStr(32), Randoms.RandomStr(32)];

    /// <summary>
    /// PBKDF2迭代次数（默认100000）
    /// </summary>
    [Description("PBKDF2迭代次数（默认100000）")]
    public int Pbkdf2Iterations { get; set; } = 100000;

    /// <summary>
    /// 派生密钥缓存过期时间（小时）
    /// </summary>
    [Description("派生密钥缓存过期时间（小时）")]
    public int CacheExpiryHours { get; set; } = 1;

    /// <summary>
    /// 是否启用密钥缓存
    /// </summary>
    [Description("是否启用密钥缓存")]
    public bool EnableKeyCache { get; set; } = true;
}