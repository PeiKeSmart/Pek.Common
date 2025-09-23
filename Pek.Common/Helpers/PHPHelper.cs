using System.Security.Cryptography;
using System.Text;

namespace Pek.Helpers;

public static class PHPHelper
{
#if (NET8_0_OR_GREATER || NETSTANDARD2_1)
    /// <summary>
    /// 使用RIPEMD160算法加密Token
    /// </summary>
    /// <param name="token">原始Token（UUID格式）</param>
    /// <param name="key">加密密钥</param>
    /// <returns>加密后的Token</returns>
    public static String EncryptToken(String token, String key)
    {
        using (var hmac = new HMACRIPEMD160(Encoding.UTF8.GetBytes(key)))
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = hmac.ComputeRaw(tokenBytes);
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
    public static Boolean VerifyToken(String inputToken, String storedEncryptedToken, String key)
    {
        var encryptedInput = EncryptToken(inputToken, key);
        return String.Equals(encryptedInput, storedEncryptedToken, StringComparison.OrdinalIgnoreCase);
    }
#endif
}