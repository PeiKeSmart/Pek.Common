using System;
using System.Text;

using Murmur;

using NewLife;
using NewLife.Caching;
using NewLife.Model;

using Pek.Infrastructure;
using Pek.Configs;
using NewLife.Log;

namespace Pek.Ids;

/// <summary>
/// 生成短惟一码
/// </summary>
public class ShortUniqueCode
{
    public static String CreateCode(Int32 Id, Int32 Length = 6)
    {
        var code = "";
        var source_string = "2YU9IP1ASDFG8QWERTHJ7KLZX4CV5B3ONM6"; //自定义35进制  
        while (Id > 0)
        {
            var mod = Id % 35;
            Id = (Id - mod) / 35;
            code = source_string.ToCharArray()[mod] + code;
        }
        return code.PadRight(Length, '0'); //不足指定位补0
    }

    public static Int32 Decode(String code)
    {
        code = new String([.. (from s in code where s != '0' select s)]);
        var num = 0;
        var source_string = "2YU9IP1ASDFG8QWERTHJ7KLZX4CV5B3ONM6";
        for (var i = 0; i < code.ToCharArray().Length; i++)
        {
            for (var j = 0; j < source_string.ToCharArray().Length; j++)
            {
                if (code.ToCharArray()[i] == source_string.ToCharArray()[j])
                {
                    num += j * Convert.ToInt32(Math.Pow(35, code.ToCharArray().Length - i - 1));
                }
            }
        }
        return num;
    }

    public static String[] ShortUrl(String url)
    {
        //可以自定义生成MD5加密字符传前的混合KEY
        var key = "DengHaoNet";
        //要使用生成URL的字符
        var chars = new String[]
        {
                "a", "b", "c", "d", "e", "f", "g", "h",
                "i", "j", "k", "l", "m", "n", "o", "p",
                "q", "r", "s", "t", "u", "v", "w", "x",
                "y", "z", "0", "1", "2", "3", "4", "5",
                "6", "7", "8", "9", "A", "B", "C", "D",
                "E", "F", "G", "H", "I", "J", "K", "L",
                "M", "N", "O", "P", "Q", "R", "S", "T",
                "U", "V", "W", "X", "Y", "Z"
        };
        //对传入网址进行MD5加密
        var hex = (key + url).MD5();
        var resUrl = new String[4];
        for (var i = 0; i < 4; i++)
        {
            //把加密字符按照8位一组16进制与0x3FFFFFFF进行位与运算
            var hexint = 0x3FFFFFFF & Convert.ToInt32($"0x{hex.Substring(i * 8, 8)}", 16);
            var outChars = String.Empty;
            for (var j = 0; j < 6; j++)
            {
                //把得到的值与0x0000003D进行位与运算，取得字符数组chars索引
                var index = 0x0000003D & hexint;
                //把取得的字符相加
                outChars += chars[index];
                //每次循环按位右移5位
                hexint >>= 5;
            }
            //把字符串存入对应索引的输出数组
            resUrl[i] = outChars;
        }
        return resUrl;
    }

    /// <summary>
    /// 转为62进制
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns></returns>
    public static String ConvertTo62(Byte[] bytes) => Base62Helper.Encode(bytes, false);

    /// <summary>
    /// 计算指定字符串的哈希值
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Byte[] GetMurmurHashBytes(String str)
    {
        var hash = MurmurHash.Create32();

        var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(str));
        return bytes;
    }

    /// <summary>
    /// 获取下一个短网址Id
    /// </summary>
    /// <param name="url"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    public static String GetNextCode(String url, Int64 salt)
    {
        var hashurl = url + salt;
        var bytes = GetMurmurHashBytes(hashurl);
        var code = ConvertTo62(bytes);
        return code;
    }

    public static IList<String> GetShortUrl(Int32 count = 1)
    {
        // 从容器中获取缓存提供者
        var provider = ObjectContainer.Provider?.GetService<ICacheProvider>();
        if (provider != null && provider.Cache != provider.InnerCache && provider.Cache is not MemoryCache)
        {
            var redis = provider.Cache;
            var config = PekSysSetting.Current;
            var localBackupCounter = config.ShortCodeCounter;

            Int64 endCounter;

            var currentCounter = redis.Get<Int64>("shortcode:counter");
            
            if (currentCounter < localBackupCounter)
            {
                // 检测到异常，需要恢复计数器，使用分布式锁保护
                var recoveryLockKey = "shortcode:recovery_lock";
                var lockTimeoutMs = 3000; // 3秒超时
                
                // 使用 AcquireLock 方式，支持阻塞等待
                using var distributedLock = provider.Cache.AcquireLock(recoveryLockKey, lockTimeoutMs);
                
                // 在锁保护下重新检查并恢复
                currentCounter = redis.Get<Int64>("shortcode:counter");
                if (currentCounter < localBackupCounter)
                {
                    var safeCounter = localBackupCounter + config.ShortCodeBackupInterval;
                    redis.Set("shortcode:counter", safeCounter);
                }
                
                // 恢复完成后，在锁内安全获取计数器
                endCounter = redis.Increment("shortcode:counter", count);
            }
            else
            {
                // 正常情况：计数器状态正常，直接获取（高性能路径）
                endCounter = redis.Increment("shortcode:counter", count);
            }

            // 异步备份配置（不阻塞主流程）
            if (ShouldBackup(endCounter, localBackupCounter, config))
            {
                Task.Run(() => BackupCounterAsync(redis, config, endCounter));
            }

            // 生成短码列表（从1位开始，充分利用短码空间）
            var result = new List<String>(count);
            for (var i = 0; i < count; i++)
            {
                var currentId = endCounter - count + 1 + i;
                result.Add(Base62Helper.Encode(currentId));
            }

            return result;
        }
        else
        {
            var lang = ObjectContainer.Provider?.GetPekService<IPekLanguage>();
            throw new XException(lang?.Translate("需要Redis支持")!);
        }
    }

    /// <summary>
    /// 判断是否需要备份计数器
    /// </summary>
    private static Boolean ShouldBackup(Int64 endCounter, Int64 localBackupCounter, PekSysSetting config)
    {
        if (config.ShortCodeBackupInterval <= 0)
            return true; // 每次都备份
            
        return endCounter - localBackupCounter >= config.ShortCodeBackupInterval;
    }

    /// <summary>
    /// 异步备份计数器（避免阻塞主流程）
    /// </summary>
    private static void BackupCounterAsync(ICache redis, PekSysSetting config, Int64 endCounter)
    {
        try
        {
            var backupLockKey = "shortcode:backup_lock";
            var lockTimeoutMs = 5000; // 备份锁超时时间更短
            
            // 使用 AcquireLock 方式
            using var distributedLock = redis.AcquireLock(backupLockKey, lockTimeoutMs);
            
            // 重新检查是否仍需要备份
            var currentConfig = PekSysSetting.Current;
            if (endCounter - currentConfig.ShortCodeCounter >= currentConfig.ShortCodeBackupInterval)
            {
                currentConfig.ShortCodeCounter = endCounter;
                currentConfig.ShortCodeCounterLastUpdate = DateTime.Now;
                currentConfig.Save();
            }
        }
        catch
        {
            // 备份失败不影响主流程，静默处理
        }
    }

    /// <summary>
    /// 将雪花ID转换为固定11位的Base62编码
    /// </summary>
    /// <param name="snowflakeId">雪花算法生成的ID</param>
    /// <returns>固定11位的Base62字符串</returns>
    public static String GetFixed11DigitCode(Int64 snowflakeId) => GetFixedLengthCode(snowflakeId, 11);

    /// <summary>
    /// 将雪花ID转换为固定长度的Base62编码
    /// </summary>
    /// <param name="snowflakeId">雪花算法生成的ID</param>
    /// <param name="fixedLength">固定长度，默认11位</param>
    /// <returns>固定长度的Base62字符串</returns>
    public static String GetFixedLengthCode(Int64 snowflakeId, Int32 fixedLength = 11)
    {
        var base62 = Base62Helper.Encode(snowflakeId);

        //XTrace.WriteLine($"获取到的雪花Id：{snowflakeId}：{base62}");

        if (base62.Length >= fixedLength)
            return base62.Substring(0, fixedLength); // 如果超过指定长度就截取

        // 不足指定长度则基于雪花ID确定性补位
        var needLength = fixedLength - base62.Length;
        var random = new Random((Int32)(snowflakeId & 0xFFFFFFFF)); // 用雪花ID的低32位做种子

        var base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var suffix = "";
        for (var i = 0; i < needLength; i++)
        {
            suffix += base62Chars[random.Next(62)];
        }

        return base62 + suffix;
    }
}