using System.Security.Cryptography;
using System.Text;

namespace Pek.Security.Crypto;

#if NET6_0_OR_GREATER
/// <summary>
/// 密码可逆加密（支持主密钥轮换）
/// 密文结构（新格式）：
///  Version(1) | KeyId(1) | Nonce(12) | Cipher | Tag(16)  => Base64
/// 算法：AES-256-GCM + PBKDF2密钥派生
/// 注意：此类仅在 .NET 6.0 及以上版本可用
/// </summary>
public static class PasswordCrypto
{
    private const byte Version = 1;
    private const int NonceSize = 12; // GCM标准随机数长度
    private const int TagSize = 16;   // GCM认证标签长度
    private const int KeySize = 32;   // AES-256密钥长度
    private const int SaltSize = 16;  // Salt长度
    private const int Pbkdf2Iterations = 100000; // PBKDF2迭代次数

    // 用于随机选择密钥的Random实例（线程安全）
    private static readonly Random _random = Random.Shared;

    static PasswordCrypto()
    {
        // 简化的构造函数，移除了无效的缓存清理任务
    }

    /// <summary>
    /// 加密密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="salt">用户特定的Salt</param>
    /// <param name="keyId">密钥ID（默认随机选择）</param>
    /// <returns>Base64编码的密文</returns>
    public static string Encrypt(string password, string salt, byte? keyId = null)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("密码不能为空", nameof(password));

        if (string.IsNullOrEmpty(salt))
            throw new ArgumentException("Salt不能为空", nameof(salt));

        var settings = PasswordCryptoSetting.Current;
        if (settings.PasswordKeys == null || settings.PasswordKeys.Length == 0)
            throw new InvalidOperationException("主密钥池未配置");

        // 使用指定的KeyId或随机选择一个密钥
        var actualKeyId = keyId ?? (byte)_random.Next(settings.PasswordKeys.Length);
        if (actualKeyId >= settings.PasswordKeys.Length)
            throw new ArgumentException($"KeyId {actualKeyId} 超出密钥池范围", nameof(keyId));

        var masterKey = settings.PasswordKeys[actualKeyId];
        var derivedKey = GetDerivedKey(masterKey, salt, actualKeyId);

        // 生成随机Nonce
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        // AES-GCM加密
        using var aes = new AesGcm(derivedKey, TagSize);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var ciphertext = new byte[passwordBytes.Length];
        var tag = new byte[TagSize];

        aes.Encrypt(nonce, passwordBytes, ciphertext, tag);

        // 构建最终密文：Version | KeyId | Nonce | Ciphertext | Tag
        var result = new byte[1 + 1 + NonceSize + ciphertext.Length + TagSize];
        var offset = 0;

        result[offset++] = Version;
        result[offset++] = actualKeyId;
        Array.Copy(nonce, 0, result, offset, NonceSize);
        offset += NonceSize;
        Array.Copy(ciphertext, 0, result, offset, ciphertext.Length);
        offset += ciphertext.Length;
        Array.Copy(tag, 0, result, offset, TagSize);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// 解密密码
    /// </summary>
    /// <param name="encryptedPassword">Base64编码的密文</param>
    /// <param name="salt">用户特定的Salt</param>
    /// <returns>明文密码</returns>
    public static string Decrypt(string encryptedPassword, string salt)
    {
        if (string.IsNullOrEmpty(encryptedPassword))
            throw new ArgumentException("加密密码不能为空", nameof(encryptedPassword));

        if (string.IsNullOrEmpty(salt))
            throw new ArgumentException("Salt不能为空", nameof(salt));

        byte[] data;
        try
        {
            data = Convert.FromBase64String(encryptedPassword);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("无效的Base64格式", nameof(encryptedPassword), ex);
        }

        // 解析密文结构
        if (data.Length < 1 + 1 + NonceSize + TagSize)
            throw new ArgumentException("密文长度不足", nameof(encryptedPassword));

        var offset = 0;
        var version = data[offset++];
        if (version != Version)
            throw new NotSupportedException($"不支持的版本: {version}");

        var keyId = data[offset++];

        var settings = PasswordCryptoSetting.Current;
        if (settings.PasswordKeys == null || keyId >= settings.PasswordKeys.Length)
            throw new ArgumentException($"KeyId {keyId} 无效或密钥已轮换", nameof(encryptedPassword));

        var nonce = new byte[NonceSize];
        Array.Copy(data, offset, nonce, 0, NonceSize);
        offset += NonceSize;

        var ciphertextLength = data.Length - offset - TagSize;
        if (ciphertextLength < 0)
            throw new ArgumentException("密文格式错误", nameof(encryptedPassword));

        var ciphertext = new byte[ciphertextLength];
        Array.Copy(data, offset, ciphertext, 0, ciphertextLength);
        offset += ciphertextLength;

        var tag = new byte[TagSize];
        Array.Copy(data, offset, tag, 0, TagSize);

        // 派生解密密钥
        var masterKey = settings.PasswordKeys[keyId];
        var derivedKey = GetDerivedKey(masterKey, salt, keyId);

        // AES-GCM解密
        using var aes = new AesGcm(derivedKey, TagSize);
        var plaintext = new byte[ciphertext.Length];

        try
        {
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch (CryptographicException ex)
        {
            throw new UnauthorizedAccessException("密码解密失败，可能是密钥错误或数据被篡改", ex);
        }
    }

    /// <summary>
    /// 批量加密（高性能）
    /// </summary>
    /// <param name="passwordSaltPairs">密码和Salt对列表</param>
    /// <param name="keyId">密钥ID</param>
    /// <returns>加密结果列表</returns>
    public static List<string> EncryptBatch(IEnumerable<(string Password, string Salt)> passwordSaltPairs, byte? keyId = null)
    {
        var results = new List<string>();
        var settings = PasswordCryptoSetting.Current;

        if (settings.PasswordKeys == null || settings.PasswordKeys.Length == 0)
            throw new InvalidOperationException("主密钥池未配置");

        var actualKeyId = keyId ?? (byte)_random.Next(settings.PasswordKeys.Length);
        var masterKey = settings.PasswordKeys[actualKeyId];

        foreach (var (password, salt) in passwordSaltPairs)
        {
            try
            {
                var encrypted = Encrypt(password, salt, actualKeyId);
                results.Add(encrypted);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"批量加密失败：{ex.Message}", ex);
            }
        }

        return results;
    }

    /// <summary>
    /// 重新加密（密钥轮换）
    /// </summary>
    /// <param name="oldEncryptedPassword">旧密文</param>
    /// <param name="salt">Salt</param>
    /// <param name="newKeyId">新密钥ID</param>
    /// <returns>新密文</returns>
    public static string ReEncrypt(string oldEncryptedPassword, string salt, byte? newKeyId = null)
    {
        // 先解密
        var plainPassword = Decrypt(oldEncryptedPassword, salt);

        // 用新密钥重新加密
        return Encrypt(plainPassword, salt, newKeyId);
    }

    /// <summary>
    /// 获取派生密钥（无缓存）
    /// </summary>
    private static byte[] GetDerivedKey(string masterKey, string salt, byte keyId)
    {
        // 使用PBKDF2派生密钥
        var saltBytes = Encoding.UTF8.GetBytes(salt + keyId); // 混合KeyId到Salt中
        using var pbkdf2 = new Rfc2898DeriveBytes(masterKey, saltBytes, Pbkdf2Iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeySize);
    }
}
#endif