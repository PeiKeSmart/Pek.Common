using NewLife.Data;

namespace Pek.Ids;

/// <summary>
/// Id 生成器
/// </summary>
public static class IdHelper
{
    public static readonly Snowflake snowflake = new Snowflake();

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
    public static string GenerateSid()
    {
        var i = 1;
        var byteArray = Guid.NewGuid().ToByteArray();
        foreach (var b in byteArray)
        {
            i *= b + 1;
        }
        return string.Format("{0:x}", i - DateTime.Now.Ticks);
    }

    /// <summary>
    /// 获取NewLife的改进雪花算法
    /// </summary>
    /// <returns></returns>
    public static Int64 GetSId()
    {
        return snowflake.NewId();
    }
}