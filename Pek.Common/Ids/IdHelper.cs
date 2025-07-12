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
    /// 配置雪花算法的WorkerId
    /// </summary>
    /// <param name="workerId">工作节点ID，范围0-1023</param>
    public static void ConfigureWorkerId(Int32 workerId)
    {
        if (workerId < 0 || workerId > 1023)
            throw new ArgumentOutOfRangeException(nameof(workerId), "WorkerId must be between 0 and 1023");
        
        snowflake.WorkerId = workerId;
    }

    /// <summary>
    /// 使用Redis等缓存系统加入集群，自动分配WorkerId
    /// </summary>
    /// <param name="cache">缓存实例</param>
    /// <param name="key">分配WorkerId的键名</param>
    public static void JoinCluster(NewLife.Caching.ICache cache, String key = "SnowflakeWorkerId") => snowflake.JoinCluster(cache, key);
}