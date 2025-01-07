using System.Security.Cryptography;
using System.Text;

namespace Pek.Security;

/// <summary>
/// 算法、加密、解密
/// </summary>
public class CalcTo
{
    /// <summary>
    /// 异或算法
    /// </summary>
    /// <param name="s">字符串</param>
    /// <param name="key">异或因子 2-253</param>
    /// <returns>返回异或后的字符串</returns>
    public static String XorKey(String s, Int32 key)
    {
        var n = key > 253 ? 253 : key < 2 ? 2 : key;
        var k = Byte.Parse(n.ToString());

        var bytes = Encoding.Unicode.GetBytes(s);
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (Byte)(bytes[i] ^ k ^ (k + 7));
        }
        return Encoding.Unicode.GetString(bytes);
    }

    /// <summary>
    /// MD5加密 小写
    /// </summary>
    /// <param name="s">需加密的字符串</param>
    /// <param name="len">长度 默认32 可选16</param>
    /// <returns></returns>
    public static String MD5(String s, Int32 len = 32)
    {
        String result;
        using var md5Hasher = new MD5CryptoServiceProvider();
        var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(s));
        var sb = new StringBuilder();
        for (var i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString("x2"));
        }
        result = sb.ToString();

        //result = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(s, "MD5").ToLower();
        return len == 32 ? result : result.Substring(8, 16);
    }

    #region DES 加解密

    /// <summary> 
    /// DES 加密 
    /// </summary> 
    /// <param name="Text">内容</param> 
    /// <param name="sKey">密钥</param> 
    /// <returns></returns> 
    public static String EnDES(String Text, String sKey)
    {
        var des = new DESCryptoServiceProvider();
        Byte[] inputByteArray;
        inputByteArray = Encoding.Default.GetBytes(Text);
        des.Key = Encoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
        des.IV = Encoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
        var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(inputByteArray, 0, inputByteArray.Length);
        cs.FlushFinalBlock();
        var ret = new StringBuilder();
        foreach (var b in ms.ToArray())
        {
            ret.AppendFormat("{0:X2}", b);
        }
        return ret.ToString();
    }

    /// <summary> 
    /// DES 解密 
    /// </summary> 
    /// <param name="Text">内容</param> 
    /// <param name="sKey">密钥</param> 
    /// <returns></returns> 
    public static String DeDES(String Text, String sKey)
    {
        var des = new DESCryptoServiceProvider();
        Int32 len;
        len = Text.Length / 2;
        var inputByteArray = new Byte[len];
        Int32 x, i;
        for (x = 0; x < len; x++)
        {
            i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
            inputByteArray[x] = (Byte)i;
        }
        des.Key = Encoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
        des.IV = Encoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
        var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(inputByteArray, 0, inputByteArray.Length);
        cs.FlushFinalBlock();
        return Encoding.Default.GetString(ms.ToArray());
    }

    #endregion

    #region SHA1 加密

    /// <summary>
    /// 20字节,160位
    /// </summary>
    /// <param name="str">内容</param>
    /// <returns></returns>
    public static String SHA128(String str)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        using var SHA1 = new SHA1CryptoServiceProvider();
        var byteArr = SHA1.ComputeHash(buffer);
        return BitConverter.ToString(byteArr);
    }

    /// <summary>
    /// 32字节,256位
    /// </summary>
    /// <param name="str">内容</param>
    /// <returns></returns>
    public static String SHA256(String str)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        using var SHA256 = new SHA256CryptoServiceProvider();
        var byteArr = SHA256.ComputeHash(buffer);
        return BitConverter.ToString(byteArr);
    }

    /// <summary>
    /// 48字节,384位
    /// </summary>
    /// <param name="str">内容</param>
    /// <returns></returns>
    public static string SHA384(string str)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        using var SHA384 = new SHA384CryptoServiceProvider();
        var byteArr = SHA384.ComputeHash(buffer);
        return BitConverter.ToString(byteArr);
    }

    /// <summary>
    /// 64字节,512位
    /// </summary>
    /// <param name="str">内容</param>
    /// <returns></returns>
    public static String SHA512(String str)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        using var SHA512 = new SHA512CryptoServiceProvider();
        var byteArr = SHA512.ComputeHash(buffer);
        return BitConverter.ToString(byteArr);
    }
    #endregion

    /// <summary>
    /// HMAC_SHA1 加密
    /// </summary>
    /// <param name="str">内容</param>
    /// <param name="key">密钥</param>
    /// <returns></returns>
    public static String HMAC_SHA1(String str, String key)
    {
        using var hmacsha1 = new HMACSHA1
        {
            Key = Encoding.UTF8.GetBytes(key)
        };
        var dataBuffer = Encoding.UTF8.GetBytes(str);
        var hashBytes = hmacsha1.ComputeHash(dataBuffer);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// HMAC_SHA256 加密
    /// </summary>
    /// <param name="str">内容</param>
    /// <param name="key">密钥</param>
    /// <returns></returns>
    public static String HMAC_SHA256(String str, String key)
    {
        using var hmacsha256 = new HMACSHA256
        {
            Key = Encoding.UTF8.GetBytes(key)
        };
        var dataBuffer = Encoding.UTF8.GetBytes(str);
        var hashBytes = hmacsha256.ComputeHash(dataBuffer);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// HMACSHA384 加密
    /// </summary>
    /// <param name="str">内容</param>
    /// <param name="key">密钥</param>
    /// <returns></returns>
    public static String HMACSHA384(String str, String key)
    {
        using var hmacsha384 = new HMACSHA384
        {
            Key = Encoding.UTF8.GetBytes(key)
        };
        var dataBuffer = Encoding.UTF8.GetBytes(str);
        var hashBytes = hmacsha384.ComputeHash(dataBuffer);
        return System.Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// HMACSHA512 加密
    /// </summary>
    /// <param name="str">内容</param>
    /// <param name="key">密钥</param>
    /// <returns></returns>
    public static String HMACSHA512(String str, String key)
    {
        using var hmacsha512 = new HMACSHA512
        {
            Key = Encoding.UTF8.GetBytes(key)
        };
        var dataBuffer = Encoding.UTF8.GetBytes(str);
        var hashBytes = hmacsha512.ComputeHash(dataBuffer);
        return System.Convert.ToBase64String(hashBytes);
    }
}