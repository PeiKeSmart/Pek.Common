using System.Runtime.Serialization;

using NewLife.Log;

namespace Pek.Exceptions;

/// <summary>
/// 表示在应用程序执行期间发生的错误
/// </summary>
[Serializable]
public class DHException : Exception
{
    /// <summary>
    /// 业务或系统自定义错误码。0 表示未指定。
    /// </summary>
    public Int32 ErrorCode { get; }

    /// <summary>
    /// 初始化Exception类的新实例.
    /// </summary>
    public DHException() : this(0) { }

    /// <summary>
    /// 使用错误码初始化Exception类的新实例.
    /// </summary>
    /// <param name="errorCode">自定义错误码.</param>
    public DHException(Int32 errorCode)
    {
        ErrorCode = errorCode;
        XTrace.WriteException(this);
    }

    /// <summary>
    /// 使用指定的错误消息初始化Exception类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    public DHException(String message)
        : this(0, message) { }

    /// <summary>
    /// 使用指定的错误码与错误消息初始化Exception类的新实例.
    /// </summary>
    /// <param name="errorCode">自定义错误码.</param>
    /// <param name="message">描述错误的消息.</param>
    public DHException(Int32 errorCode, String message)
        : base(message)
    {
        ErrorCode = errorCode;
        if (errorCode != 0)
            XTrace.Log.Error("[ErrorCode:{0}] {1}\r\n{2}", errorCode, message, this);
        else
            XTrace.Log.Error("{0}\r\n{1}", message, this);
    }

    /// <summary>
    /// 使用指定的错误消息初始化Exception类的新实例.
    /// </summary>
    /// <param name="messageFormat">异常消息格式.</param>
    /// <param name="args">异常消息参数.</param>
    public DHException(String messageFormat, params Object[] args)
        : this(0, messageFormat, args) { }

    /// <summary>
    /// 使用错误码和格式化消息初始化.
    /// </summary>
    /// <param name="errorCode">自定义错误码.</param>
    /// <param name="messageFormat">异常消息格式.</param>
    /// <param name="args">异常消息参数.</param>
    public DHException(Int32 errorCode, String messageFormat, params Object[] args)
        : base(String.Format(messageFormat, args))
    {
        ErrorCode = errorCode;
        if (errorCode != 0)
            XTrace.Log.Error("[ErrorCode:{0}] " + messageFormat, PrependArg(errorCode, args));
        else
            XTrace.Log.Error(messageFormat, args);
    }

    /// <summary>
    /// 使用序列化的数据初始化Exception类的新实例.
    /// </summary>
    /// <param name="info">包含有关引发异常的序列化对象数据的序列化信息.</param>
    /// <param name="context">包含有关源或目标的上下文信息的流上下文.</param>
    protected DHException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        try
        {
            ErrorCode = info.GetInt32(nameof(ErrorCode));
        }
        catch { /* 兼容旧序列化数据 */ }
    }

    /// <summary>
    /// 使用指定的错误消息和对引起该异常的内部异常的引用来初始化Exception类的新实例。.
    /// </summary>
    /// <param name="message">解释异常原因的错误消息.</param>
    /// <param name="innerException">导致当前异常的异常；如果未指定内部异常，则为null引用.</param>
    public DHException(String message, Exception innerException)
        : this(0, message, innerException) { }

    /// <summary>
    /// 使用错误码、错误消息和内部异常初始化。
    /// </summary>
    /// <param name="errorCode">自定义错误码.</param>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public DHException(Int32 errorCode, String message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        if (errorCode != 0)
            XTrace.Log.Error("[ErrorCode:{0}] {1}\r\n{2}", errorCode, message, innerException);
        else
            XTrace.Log.Error("{0}\r\n{1}", message, innerException);
    }

    /// <summary>
    /// 序列化支持，写入自定义字段。
    /// </summary>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }

    private static Object[] PrependArg(Int32 errorCode, Object[] args)
    {
        if (args == null || args.Length == 0) return new Object[] { errorCode };
        var arr = new Object[args.Length + 1];
        arr[0] = errorCode;
        Array.Copy(args, 0, arr, 1, args.Length);
        return arr;
    }
}