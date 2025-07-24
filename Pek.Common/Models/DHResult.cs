using NewLife;

using Pek.Helpers;

namespace Pek.Models;

/// <summary>
/// APP专用返回（泛型版本）
/// </summary>
/// <typeparam name="T1">Data 的类型</typeparam>
/// <typeparam name="T2">ExtData 的类型</typeparam>
public class DHResult<T1, T2>
{
    /// <summary>
    /// 状态码
    /// </summary>
    public StateCode Code { get; set; } = StateCode.Fail;

    /// <summary>
    /// 错误码
    /// </summary>
    public Int32 ErrCode { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public String? Message { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public T1? Data { get; set; }

    /// <summary>
    /// 其他数据
    /// </summary>
    public T2? ExtData { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    public DateTime OperationTime { get; set; }

    /// <summary>
    /// 标识
    /// </summary>
    public String? Id { get; set; }

    /// <summary>
    /// 初始化返回结果
    /// </summary>
    public DHResult()
    {
        Code = StateCode.Fail;
        OperationTime = DateTime.Now;
    }

    /// <summary>
    /// 初始化返回结果
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    /// <param name="extdata">其他数据</param>
    public DHResult(StateCode code, String message, T1? data = default, T2? extdata = default)
    {
        Code = code;
        Message = message;
        Data = data;
        OperationTime = DateTime.Now;
        ExtData = extdata;
        Id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 获取结果对象（用于序列化）
    /// </summary>
    public Object GetResult()
    {
        if (Id.IsNullOrWhiteSpace())
            Id = Guid.NewGuid().ToString();

        return new
        {
            Code = Code.Value(),
            Message,
            OperationTime,
            Data,
            ExtData,
            Id,
            ErrCode,
        };
    }
}

/// <summary>
/// APP专用返回（单泛型版本）
/// </summary>
/// <typeparam name="T">Data 的类型</typeparam>
public class DHResult<T> : DHResult<T, Object?>
{
    /// <summary>
    /// 初始化返回结果
    /// </summary>
    public DHResult() : base() { }

    /// <summary>
    /// 初始化返回结果
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    /// <param name="extdata">其他数据</param>
    public DHResult(StateCode code, String message, T? data = default, Object? extdata = null)
        : base(code, message, data, extdata) { }
}

/// <summary>
/// APP专用返回（非泛型版本，向后兼容）
/// </summary>
public class DHResult : DHResult<Object?, Object?>
{
    /// <summary>
    /// 初始化返回结果
    /// </summary>
    public DHResult() : base() { }

    /// <summary>
    /// 初始化返回结果
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    /// <param name="extdata">其他数据</param>
    public DHResult(StateCode code, String message, Object? data = null, Object? extdata = null)
        : base(code, message, data, extdata) { }
}