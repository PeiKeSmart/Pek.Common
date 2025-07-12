using NewLife.Data;

namespace Pek.Ids;

/// <summary>
/// Id 生成器
/// </summary>
public static class IdHelper
{
    public static readonly Snowflake snowflake = new();

#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
    /// <summary>
    /// 微软获取新的13位Id字符串
    /// </summary>
    /// <example>0HLV413GIHKK5</example>
    /// <returns></returns>
    public static String GetNextId() => CorrelationIdGenerator.GetNextId();

    /// <summary>
    /// 微软获取Id的字符串方法
    /// </summary>
    /// <returns></returns>
    public static String GetIdString() => FastGuid.NewGuid().IdString;
#endif

    /// <summary>
    /// 生成sessionid
    /// </summary>
    /// <example>62acfda11f5a4b3c</example>
    public static String GenerateSid()
    {
        var i = 1;
        var byteArray = Guid.NewGuid().ToByteArray();
        foreach (var b in byteArray)
        {
            i *= b + 1;
        }
        return String.Format("{0:x}", i - DateTime.Now.Ticks);
    }

    /// <summary>
    /// 获取NewLife的改进雪花算法
    /// </summary>
    /// <returns></returns>
    public static Int64 GetSId() => snowflake.NewId();

    /// <summary>
    /// 获取雪花算法生成的Base62格式的短ID
    /// </summary>
    /// <returns>Base62编码的短ID字符串，通常为10-11位</returns>
    public static String GetShortId() => Base62Helper.Encode(snowflake.NewId());

    /// <summary>
    /// 将雪花算法生成的Int64 ID转换为Base62字符串
    /// </summary>
    /// <param name="snowflakeId">雪花算法生成的ID</param>
    /// <returns>Base62编码的字符串</returns>
    public static String ConvertSnowflakeToBase62(Int64 snowflakeId) => Base62Helper.Encode(snowflakeId);

    /// <summary>
    /// 将Base62字符串转换回雪花算法的Int64 ID
    /// </summary>
    /// <param name="base62String">Base62编码的字符串</param>
    /// <returns>原始的雪花算法ID</returns>
    public static Int64 ConvertBase62ToSnowflake(String base62String) => Base62Helper.DecodeToInt64(base62String);
}