using System.Text;

namespace Pek.IO;

/// <summary>
/// 文件操作辅助类 - 转换
/// </summary>
public static partial class FileUtil
{
    #region ToString(转换成字符串)        

    /// <summary>
    /// 字节数组转换成字符串
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="encoding">字符编码</param>
    /// <returns></returns>
    public static String ToString(Byte[] data, Encoding? encoding = null)
    {
        if (data == null || data.Length == 0)
        {
            return String.Empty;
        }

        encoding ??= Encoding.UTF8;

        return encoding.GetString(data);
    }

    /// <summary>
    /// 流转换成字符串
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="encoding">字符串编码</param>
    /// <param name="bufferSize">缓冲区大小</param>
    /// <param name="isCloseStream">读取完成是否释放流，默认为true</param>
    /// <returns></returns>
    public static String ToString(Stream stream, Encoding? encoding = null, Int32 bufferSize = 1024 * 2,
        Boolean isCloseStream = true)
    {
        if (stream == null)
        {
            return String.Empty;
        }

        encoding ??= Encoding.UTF8;

        if (stream.CanRead == false)
        {
            return String.Empty;
        }

        using var reader = new StreamReader(stream, encoding, true, bufferSize, !isCloseStream);
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var result = reader.ReadToEnd();
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return result;
    }

    /// <summary>
    /// 流转换成字符串
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="encoding">字符串编码</param>
    /// <param name="bufferSize">缓冲区大小</param>
    /// <param name="isCloseStream">读取完成是否释放流，默认为true</param>
    /// <returns></returns>
    public static async Task<String> ToStringAsync(Stream stream, Encoding? encoding = null,
        Int32 bufferSize = 1024 * 2,
        Boolean isCloseStream = true)
    {
        if (stream == null)
        {
            return String.Empty;
        }

        encoding ??= Encoding.UTF8;

        if (stream.CanRead == false)
        {
            return String.Empty;
        }

        using var reader = new StreamReader(stream, encoding, true, bufferSize, !isCloseStream);
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var result = await reader.ReadToEndAsync().ConfigureAwait(false);
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return result;
    }

    #endregion

    #region ToStream(转换成流)

    /// <summary>
    /// 字符串转换成流
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="encoding">字符编码</param>
    /// <returns></returns>
    public static Stream ToStream(String data, Encoding? encoding = null)
    {
        if (String.IsNullOrWhiteSpace(data))
        {
            return Stream.Null;
        }

        encoding ??= Encoding.UTF8;

        return new MemoryStream(ToBytes(data, encoding));
    }

    #endregion

    #region ToBytes(转换成字节数组)

    /// <summary>
    /// 字符串转换为字节数组
    /// </summary>
    /// <param name="data">数据。默认字符编码：utf-8</param>
    public static Byte[] ToBytes(String data) => ToBytes(data, Encoding.UTF8);

    /// <summary>
    /// 字符串转换成字节数组
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="encoding">字符编码</param>
    public static Byte[] ToBytes(String data, Encoding encoding)
    {
        if (String.IsNullOrWhiteSpace(data))
        {
            return [];
        }
        return encoding.GetBytes(data);
    }

    /// <summary>
    /// 流转换成字节流
    /// </summary>
    /// <param name="stream">流</param>
    public static Byte[] ToBytes(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var buffer = new Byte[stream.Length];
        _ = stream.Read(buffer, 0, buffer.Length);
        return buffer;
    }

    /// <summary>
    /// 流转换成字节流
    /// </summary>
    /// <param name="stream">流</param>
    public static async Task<Byte[]> ToBytesAsync(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var buffer = new Byte[stream.Length];
        _ = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        return buffer;
    }

    #endregion
}