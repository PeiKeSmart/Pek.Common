namespace Pek.IO;

/// <summary>
/// 文件操作帮助类 - 加载
/// </summary>
public static partial class FileUtil
{
    #region ReadAllText(读取文件所有文本)

    /// <summary>
    /// 读取文件所有文本
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static async Task<String> ReadAllTextAsync(String filePath)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));

        using var reader = File.OpenText(filePath);
        return await reader.ReadToEndAsync();
    }

    #endregion

    #region ReadAllBytes(读取文件所有字节)

    /// <summary>
    /// 读取文件所有字节
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static async Task<Byte[]> ReadAllBytesAsync(String filePath)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        using var stream = File.Open(filePath, FileMode.Open);
        var result = new Byte[stream.Length];
        _ = await stream.ReadAsync(result, 0, (Int32)stream.Length);
        return result;
    }

    #endregion
}
