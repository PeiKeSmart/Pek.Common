using System.Security.Cryptography;
using System.Text;

namespace Pek;

/// <summary>
/// 字节流(<see cref="Stream"/>) 扩展
/// </summary>
public static class StreamExtensions
{
    #region ToFile(将流写入指定文件路径)

    /// <summary>
    /// 将流写入指定文件路径
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public static Boolean ToFile(this Stream stream, String path)
    {
        if (stream == null)
        {
            return false;
        }

        const Int32 bufferSize = 32768;
        var result = true;
        Stream? fileStream = null;
        var buffer = new Byte[bufferSize];
        try
        {
            using (fileStream = File.OpenWrite(path))
            {
                Int32 len;
                while ((len = stream.Read(buffer, 0, bufferSize)) > 0)
                {
                    fileStream.Write(buffer, 0, len);
                }
            }
        }
        catch
        {
            result = false;
        }
        finally
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        return (result && File.Exists(path));
    }

    #endregion

    #region ContentsEqual(比较流内容是否相等)

    /// <summary>
    /// 比较流内容是否相等
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="other">待比较的流</param>
    /// <returns></returns>
    public static Boolean ContentsEqual(this Stream stream, Stream other)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (other == null) throw new ArgumentNullException(nameof(other));

        if (stream.Length != other.Length)
        {
            return false;
        }

        const Int32 bufferSize = 2048;
        var streamBuffer = new Byte[bufferSize];
        var otherBuffer = new Byte[bufferSize];

        while (true)
        {
            var streamLen = stream.Read(streamBuffer, 0, bufferSize);
            var otherLen = other.Read(otherBuffer, 0, bufferSize);

            if (streamLen != otherLen)
            {
                return false;
            }

            if (streamLen == 0)
            {
                return true;
            }

            var iterations = (Int32)Math.Ceiling((Double)streamLen / sizeof(Int64));
            for (var i = 0; i < iterations; i++)
            {
                if (BitConverter.ToInt64(streamBuffer, i * sizeof(Int64)) !=
                    BitConverter.ToInt64(otherBuffer, i * sizeof(Int64)))
                {
                    return false;
                }
            }
        }
    }

    #endregion

    #region GetReader(获取流读取器)

    /// <summary>
    /// 获取流读取器，默认编码：UTF-8
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static StreamReader GetReader(this Stream stream) => GetReader(stream, null);

    /// <summary>
    /// 获取流读取器，使用指定编码
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="encoding">编码，默认：UTF-8</param>
    /// <returns></returns>
    public static StreamReader GetReader(this Stream stream, Encoding? encoding)
    {
        if (stream.CanRead == false)
        {
            throw new InvalidOperationException("Stream 不支持读取操作");
        }
        encoding ??= Encoding.UTF8;
        return new StreamReader(stream, encoding);
    }

    #endregion

    #region GetWriter(获取流写入器)

    /// <summary>
    /// 获取流写入器，默认编码：UTF-8
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static StreamWriter GetWriter(this Stream stream) => GetWriter(stream, null);

    /// <summary>
    /// 获取流写入器，使用指定编码
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="encoding">编码，默认编码：UTF-8</param>
    /// <returns></returns>
    public static StreamWriter GetWriter(this Stream stream, Encoding? encoding)
    {
        if (stream.CanWrite == false)
        {
            throw new InvalidOperationException("Stream 不支持写入操作");
        }

        encoding ??= Encoding.UTF8;
        return new StreamWriter(stream, encoding);
    }

    #endregion

    #region ReadToEnd(读取所有文本)

    /// <summary>
    /// 从流中读取所有文本，默认编码：UTF-8
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static String ReadToEnd(this Stream stream) => ReadToEnd(stream, null);

    /// <summary>
    /// 从流中读取所有文本，使用指定编码
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="encoding">编码，默认编码：UTF-8</param>
    /// <returns></returns>
    public static String ReadToEnd(this Stream stream, Encoding? encoding)
    {
        using var reader = stream.GetReader(encoding);
        return reader.ReadToEnd();
    }

    #endregion

    #region SeekToBegin(设置流指针指向流的开始位置)

    /// <summary>
    /// 设置流指针指向流的开始位置
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static Stream SeekToBegin(this Stream stream)
    {
        if (stream.CanSeek == false)
        {
            throw new InvalidOperationException("Stream 不支持寻址操作");
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    #endregion

    #region SeekToEnd(设置流指针指向流的结束位置)

    /// <summary>
    /// 设置流指针指向流的结束位置
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static Stream SeekToEnd(this Stream stream)
    {
        if (stream.CanSeek == false)
        {
            throw new InvalidOperationException("Stream 不支持寻址操作");
        }

        stream.Seek(0, SeekOrigin.End);
        return stream;
    }

    #endregion

    #region CopyToMemory(复制流到内存流)

    /// <summary>
    /// 将流复制到内存流中
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static MemoryStream CopyToMemory(this Stream stream)
    {
        var memoryStream = new MemoryStream((Int32)stream.Length);
        stream.CopyTo(memoryStream);
        return memoryStream;
    }

    #endregion

    #region ReadAllBytes(将流写入字节数组)

    /// <summary>
    /// 将流写入字节数组
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static Byte[] ReadAllBytes(this Stream stream)
    {
        using var memoryStream = stream.CopyToMemory();
        return memoryStream.ToArray();
    }

    #endregion

    #region Write(将字节数组写入流)

    /// <summary>
    /// 将字节数组写入流
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="bytes">字节数组</param>
    public static void DGWrite(this Stream stream, Byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

    #endregion

    #region Write(将字符串写入流)

    /// <summary>
    /// 将字符串写以指定编码方式写入流
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="context"></param>
    /// <param name="encoding"></param>
    public static void DGWrite(this Stream stream, String context, Encoding encoding)
    {
        var buffer = encoding.GetBytes(context);
        stream.Write(buffer, 0, buffer.Length);
    }

    #endregion

    #region GetMd5(获取流的MD5值)

    /// <summary>
    /// 获取流的MD5值
    /// </summary>
    /// <param name="stream">流</param>
    public static String GetMd5(this Stream stream)
    {
        using var md5 = MD5.Create();
        var buffer = md5.ComputeHash(stream);
        var md5Builder = new StringBuilder();
        foreach (var b in buffer)
        {
            md5Builder.Append(b.ToString("x2"));
        }

        return md5Builder.ToString();
    }

    #endregion

    /// <summary>
    /// 将流转换为内存流
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static MemoryStream SaveAsMemoryStream(this Stream stream)
    {
        stream.Position = 0;
        return new MemoryStream(stream.ToArray());
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static Byte[] ToArray(this Stream stream)
    {
        stream.Position = 0;
        var bytes = new Byte[stream.Length];
        _ = stream.Read(bytes, 0, bytes.Length);

        // 设置当前流的位置为流的开始
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Byte[]> ToArrayAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        stream.Position = 0;
        var bytes = new Byte[stream.Length];
        await stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }

    /// <summary>
    /// Copy to target stream, while flushing the data after every operation.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="bufferSize">The copy buffer size.</param>
    /// <param name="flush">Indicates whether the data should be flushed after every operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, bool flush, CancellationToken cancellationToken)
    {
        if (!flush)
        {
            await source.CopyToAsync(destination, bufferSize, cancellationToken).ConfigureAwait(false);
            return;
        }

        var buffer = new byte[bufferSize];
        int readCount;
        while ((readCount = await source.ReadAsync(buffer, 0, bufferSize, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, readCount, cancellationToken).ConfigureAwait(false);
            await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}