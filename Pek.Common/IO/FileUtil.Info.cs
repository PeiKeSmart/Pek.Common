using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

using Pek.Mime;

using FileInfo = System.IO.FileInfo;

namespace Pek.IO;

/// <summary>
/// 文件操作辅助类 - 信息
/// </summary>
public static partial class FileUtil
{
    #region GetExtension(获取文件扩展名)

    /// <summary>
    /// 获取文件扩展名。例如：a.txt => txt
    /// </summary>
    /// <param name="fileNameWithExtension">文件名。包含扩展名</param>
    public static String GetExtension(String? fileNameWithExtension)
    {
        if (fileNameWithExtension == null) throw new ArgumentNullException(nameof(fileNameWithExtension));

        var lastDotIndex = fileNameWithExtension.LastIndexOf('.');
        if (lastDotIndex < 0)
            return String.Empty;
        return fileNameWithExtension[(lastDotIndex + 1)..];
    }

    #endregion

    #region GetContentType(根据扩展名获取文件内容类型)

    /// <summary>
    /// 根据扩展名获取文件内容类型
    /// </summary>
    /// <param name="ext">扩展名</param>
    /// <returns></returns>
    public static String? GetContentType(String ext)
    {
        var dict = MimeMapper.MimeTypes;
        ext = ext.ToLower();
        if (!ext.StartsWith("."))
        {
            ext = "." + ext;
        }

        dict.TryGetValue(ext, out var contentType);
        return contentType;
    }

    #endregion

    #region GetFileSize(获取文件大小)

    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns></returns>
    public static FileSize GetFileSize(String filePath)
    {
        if (String.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        return GetFileSize(new FileInfo(filePath));
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="fileInfo">文件信息</param>
    /// <returns></returns>
    public static FileSize GetFileSize(FileInfo fileInfo) => fileInfo == null ? throw new ArgumentNullException(nameof(fileInfo)) : new FileSize(fileInfo.Length);

    #endregion

    #region GetVersion(获取文件版本号)

    /// <summary>
    /// 获取文件版本号
    /// </summary>
    /// <param name="fileName">完整文件名</param>
    /// <returns></returns>
    public static String? GetVersion(String fileName)
    {
        if (File.Exists(fileName))
        {
            var fvi = FileVersionInfo.GetVersionInfo(fileName);
            return fvi.FileVersion;
        }

        return null;
    }

    #endregion

    #region GetEncoding(获取文件编码)

    /// <summary>
    /// 获取文件编码
    /// </summary>
    /// <param name="filePath">文件绝对路径</param>
    /// <returns></returns>
    public static Encoding GetEncoding(String filePath) => GetEncoding(filePath, Encoding.Default);

    /// <summary>
    /// 获取文件编码
    /// </summary>
    /// <param name="filePath">文件绝对路径</param>
    /// <param name="defaultEncoding">默认编码</param>
    /// <returns></returns>
    public static Encoding GetEncoding(String filePath, Encoding defaultEncoding)
    {
        var targetEncoding = defaultEncoding;
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4))
        {
            if (fs != null && fs.Length >= 2)
            {
                var pos = fs.Position;
                fs.Position = 0;
                var buffer = new Int32[4];
                buffer[0] = fs.ReadByte();
                buffer[1] = fs.ReadByte();
                buffer[2] = fs.ReadByte();
                buffer[3] = fs.ReadByte();
                fs.Position = pos;

                if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    targetEncoding = Encoding.BigEndianUnicode;
                }

                if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    targetEncoding = Encoding.Unicode;
                }

                if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                {
                    targetEncoding = Encoding.UTF8;
                }
            }
        }

        return targetEncoding;
    }

    #endregion

    #region GetMd5(获取文件的MD5值)

    /// <summary>
    /// 获取文件的MD5值
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns></returns>
    public static String GetMd5(String file) => HashFile(file, "md5");

    /// <summary>
    /// 计算文件的哈希值
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="algName">算法名。例如：md5,sha1</param>
    /// <returns></returns>
    private static String HashFile(String file, String algName)
    {
        if (!File.Exists(file))
        {
            return String.Empty;
        }

        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        var bytes = HashData(fs, algName);
        return ToHexString(bytes);
    }

    /// <summary>
    /// 计算哈希值
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="algName">算法名。例如：md5,sha1</param>
    /// <returns></returns>
    private static Byte[] HashData(Stream stream, String algName)
    {
        if (String.IsNullOrWhiteSpace(algName))
        {
            throw new ArgumentNullException(nameof(algName));
        }

        HashAlgorithm algorithm;
        if (String.Compare(algName, "sha1", StringComparison.OrdinalIgnoreCase) == 0)
        {
            algorithm = SHA1.Create();
        }
        else if (String.Compare(algName, "md5", StringComparison.OrdinalIgnoreCase) == 0)
        {
            algorithm = MD5.Create();
        }
        else
        {
            throw new ArgumentException($"{nameof(algName)} 只能使用 sha1 或 md5.");
        }

        var bytes = algorithm.ComputeHash(stream);
        algorithm.Dispose();
        return bytes;
    }

    /// <summary>
    /// 将字节数组转换为16进制表示的字符在
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns></returns>
    private static String ToHexString(Byte[] bytes) => BitConverter.ToString(bytes).Replace("-", "");

    #endregion

    #region GetSha1(获取文件的SHA1值)

    /// <summary>
    /// 获取文件的SHA1值
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns></returns>
    public static String GetSha1(String file) => HashFile(file, "sha1");

    #endregion
}

