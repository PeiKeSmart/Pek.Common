#if NET8_0_OR_GREATER
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Pek.Compress.StringZipper;

/// <summary>
/// 字符串压缩工具
/// </summary>
public static class DHStringZipper
{
    private static void Register(string identifier, object component)
    {
        var syncRoot = SyncRoot;
        lock (syncRoot)
        {
            var sortedDictionary = new SortedDictionary<string, object>(_components, StringComparer.OrdinalIgnoreCase);
            sortedDictionary[identifier] = component;
            _components = sortedDictionary;
        }
    }

    /// <summary>
    /// 注册一个压缩器
    /// </summary>
    /// <param name="compresser"></param>
    /// <exception cref="T:System.ArgumentNullException"></exception>
    public static void Register(ICompressor compresser)
    {
        if (compresser == null)
        {
            throw new ArgumentNullException("compresser");
        }
        Register(compresser.Identifier, compresser);
    }

    /// <summary>
    /// 注册一个编码器
    /// </summary>
    /// <param name="encoder"></param>
    /// <exception cref="T:System.ArgumentNullException"></exception>
    public static void Register(IEncoder encoder)
    {
        if (encoder == null)
        {
            throw new ArgumentNullException("encoder");
        }
        Register(encoder.Identifier, encoder);
    }

    public static bool TryGetComponent<T>(string id, out T component)
    {
        object value;
        if (_components.TryGetValue(id, out value) && value is T)
        {
            var cast = (T)((object)value);
            component = cast;
            return true;
        }
        component = default(T);
        return false;
    }

    /// <summary>
    /// 压缩字符串
    /// </summary>
    /// <param name="str">待压缩字符串</param>
    /// <param name="compressor">压缩器</param>
    /// <param name="encoder">编码器</param>
    /// <returns>压缩后的字符串</returns>
    /// <exception cref="T:System.ArgumentNullException"></exception>
    public static string Zip(string str, ICompressor compressor, IEncoder encoder)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }
        if (compressor == null)
        {
            throw new ArgumentNullException("compressor");
        }
        if (encoder == null)
        {
            throw new ArgumentNullException("encoder");
        }
        if (PrefixRegex.IsMatch(str))
        {
            return str;
        }
        var data = compressor.Compress(str);
        var output = encoder.Encode(data);

        var defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 3);
        defaultInterpolatedStringHandler.AppendLiteral("data:text/x-");
        defaultInterpolatedStringHandler.AppendFormatted(compressor.Identifier);
        defaultInterpolatedStringHandler.AppendLiteral(";");
        defaultInterpolatedStringHandler.AppendFormatted(encoder.Identifier);
        defaultInterpolatedStringHandler.AppendLiteral(",");
        defaultInterpolatedStringHandler.AppendFormatted(output);
        return defaultInterpolatedStringHandler.ToStringAndClear();
    }

    /// <summary>
    /// 使用默认配置压缩字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Zip(string str)
    {
        return Zip(str, Deflate, Ascii85);
    }

    /// <summary>
    /// 解压缩字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    /// <exception cref="T:System.Exception"></exception>
    public static string Unzip(string str)
    {
        var i = PrefixRegex.Match(str);
        if (!i.Success)
        {
            return str;
        }
        var strCompresser = i.Groups["Compresser"].Value;
        var strEncoder = i.Groups["Encoder"].Value;
        var strData = i.Groups["Data"].Value;
        ICompressor compresser;
        if (!TryGetComponent<ICompressor>(strCompresser, out compresser))
        {
            throw new Exception("压缩方式不支持:" + strCompresser);
        }
        IEncoder encoder;
        if (!TryGetComponent<IEncoder>(strEncoder, out encoder))
        {
            throw new Exception("编码方式不支持:" + strEncoder);
        }
        var bytes = encoder.Decode(strData);
        return compresser.Decompress(bytes);
    }

    public static IEncoder Base16
    {
        get
        {
            return Base16Encoder.Instance;
        }
    }

    public static IEncoder Base62
    {
        get
        {
            return Base62Encoder.Instance;
        }
    }

    public static IEncoder Base64
    {
        get
        {
            return Base64Encoder.Instance;
        }
    }

    public static IEncoder Ascii85
    {
        get
        {
            return Ascii85Encoder.Instance;
        }
    }

    public static ICompressor LzString
    {
        get
        {
            return LzStringCompressor.Instance;
        }
    }

    public static ICompressor Deflate
    {
        get
        {
            return DeflateStreamCompressor.Instance;
        }
    }

    public static ICompressor GZip
    {
        get
        {
            return GZipStreamCompressor.Instance;
        }
    }

    public static ICompressor Br
    {
        get
        {
            return BrotliStreamCompressor.Instance;
        }
    }

    private static readonly object SyncRoot = new object();

    private static IDictionary<string, object> _components = new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            {
                LzString.Identifier,
                LzString
            },
            {
                Deflate.Identifier,
                Deflate
            },
            {
                GZip.Identifier,
                GZip
            },
            {
                Br.Identifier,
                Br
            },
            {
                Base16.Identifier,
                Base16
            },
            {
                Base62.Identifier,
                Base62
            },
            {
                Base64.Identifier,
                Base64
            },
            {
                Ascii85.Identifier,
                Ascii85
            }
        };

    private static readonly Regex PrefixRegex = new Regex("^data:text/x-(?<Compresser>\\w+);(?<Encoder>\\w+),(?<Data>.*)", RegexOptions.Compiled);

    /// <summary>
    /// 压缩器
    /// </summary>
    public interface ICompressor
    {
        string Identifier { get; }

        byte[] Compress(string value);

        string Decompress(byte[] data);
    }

    /// <summary>
    /// 编码器
    /// </summary>
    public interface IEncoder
    {
        string Identifier { get; }

        string Encode(byte[] data);

        byte[] Decode(string value);
    }

    private class Base16Encoder : IEncoder
    {
        public static Base16Encoder Instance { get; } = new Base16Encoder();

        public string Identifier
        {
            get
            {
                return "base16";
            }
        }

        public byte[] Decode(string value)
        {
            return Util.Base16.FromBase16(value);
        }

        public string Encode(byte[] original)
        {
            return Util.Base16.ToBase16(original);
        }
    }

    private class Base62Encoder : IEncoder
    {
        public static Base62Encoder Instance { get; } = new Base62Encoder();

        public string Identifier
        {
            get
            {
                return "base62";
            }
        }

        public byte[] Decode(string value)
        {
            return Ids.Base62Helper.FromBase62(value, false);
        }

        public string Encode(byte[] data)
        {
            return Ids.Base62Helper.ToBase62(data, false);
        }
    }

    private class Base64Encoder : IEncoder
    {
        public static Base64Encoder Instance { get; } = new Base64Encoder();

        public string Identifier
        {
            get
            {
                return "base64";
            }
        }

        public byte[] Decode(string value)
        {
            return Convert.FromBase64String(value);
        }

        public string Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }
    }

    private class Ascii85Encoder : IEncoder
    {
        public static Ascii85Encoder Instance { get; } = new Ascii85Encoder();

        public string Identifier
        {
            get
            {
                return "ascii85";
            }
        }

        public byte[] Decode(string value)
        {
            return Util.Ascii85.FromAscii85String(value);
        }

        public string Encode(byte[] data)
        {
            return Util.Ascii85.ToAscii85String(data);
        }
    }

    private class LzStringCompressor : ICompressor
    {
        public static LzStringCompressor Instance { get; } = new LzStringCompressor();

        public string Identifier
        {
            get
            {
                return "lzstring";
            }
        }

        public byte[] Compress(string value)
        {
            return Util.LzString.CompressToUint8Array(value);
        }

        public string Decompress(byte[] data)
        {
            return Util.LzString.DecompressFromUint8Array(data);
        }
    }

    public abstract class StreamCompressor<TStream> : ICompressor where TStream : Stream
    {
        public abstract string Identifier { get; }

        protected abstract TStream Create(Stream stream, CompressionMode mode, bool leaveOpen);

        protected virtual Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public virtual byte[] Compress(string value)
        {
            byte[] result;
            using (var o = new MemoryStream(Encoding.GetBytes(value)))
            {
                using (var t = new MemoryStream())
                {
                    using (Stream c = Create(t, CompressionMode.Compress, true))
                    {
                        o.CopyTo(c);
                    }
                    result = t.ToArray();
                }
            }
            return result;
        }

        public virtual string Decompress(byte[] data)
        {
            string @string;
            using (var o = new MemoryStream(data))
            {
                using (var t = new MemoryStream())
                {
                    using (Stream u = Create(o, CompressionMode.Decompress, true))
                    {
                        u.CopyTo(t);
                    }
                    var bytes = t.ToArray();
                    @string = Encoding.GetString(bytes);
                }
            }
            return @string;
        }
    }

    private class DeflateStreamCompressor : StreamCompressor<DeflateStream>
    {
        public static DeflateStreamCompressor Instance { get; } = new DeflateStreamCompressor();

        public override string Identifier
        {
            get
            {
                return "deflate";
            }
        }

        protected override DeflateStream Create(Stream stream, CompressionMode mode, bool leaveOpen)
        {
            return new DeflateStream(stream, mode, leaveOpen);
        }
    }

    private class GZipStreamCompressor : StreamCompressor<GZipStream>
    {
        public static GZipStreamCompressor Instance { get; } = new GZipStreamCompressor();

        public override string Identifier
        {
            get
            {
                return "gzip";
            }
        }

        protected override GZipStream Create(Stream stream, CompressionMode mode, bool leaveOpen)
        {
            return new GZipStream(stream, mode, leaveOpen);
        }
    }

    private class BrotliStreamCompressor : StreamCompressor<BrotliStream>
    {
        public static BrotliStreamCompressor Instance { get; } = new BrotliStreamCompressor();

        public override string Identifier
        {
            get
            {
                return "br";
            }
        }

        protected override BrotliStream Create(Stream stream, CompressionMode mode, bool leaveOpen)
        {
            return new BrotliStream(stream, mode, leaveOpen);
        }
    }
}
#endif