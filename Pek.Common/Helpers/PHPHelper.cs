using System;
using System.Security.Cryptography;
using System.Text;

namespace Pek.Helpers;

public static class PHPHelper
{
    /// <summary>
    /// 使用RIPEMD160算法加密Token
    /// </summary>
    /// <param name="token">原始Token（UUID格式）</param>
    /// <param name="key">加密密钥</param>
    /// <returns>加密后的Token</returns>
    public static string EncryptToken(string token, string key)
    {
        using (var hmac = new HMACRIPEMD160(Encoding.UTF8.GetBytes(key)))
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            byte[] hashBytes = hmac.ComputeHash(tokenBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    /// <summary>
    /// 验证Token是否匹配
    /// </summary>
    /// <param name="inputToken">用户输入的Token</param>
    /// <param name="storedEncryptedToken">数据库中存储的加密Token</param>
    /// <param name="key">加密密钥</param>
    /// <returns>是否匹配</returns>
    public static bool VerifyToken(string inputToken, string storedEncryptedToken, string key)
    {
        string encryptedInput = EncryptToken(inputToken, key);
        return string.Equals(encryptedInput, storedEncryptedToken, StringComparison.OrdinalIgnoreCase);
    }
}
