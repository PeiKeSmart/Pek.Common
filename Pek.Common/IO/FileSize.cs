using Pek.Helpers;

namespace Pek.IO;

/// <summary>
/// 文件大小
/// </summary>
public struct FileSize
{
    /// <summary>
    /// 文件字节长度
    /// </summary>
    public Int64 Size { get; }

    /// <summary>
    /// 初始化一个<see cref="FileSize"/>类型的实例
    /// </summary>
    /// <param name="size">文件大小</param>
    /// <param name="unit">文件大小单位</param>
    public FileSize(Int64 size, FileSizeUnit unit = FileSizeUnit.Byte) => Size = GetSize(size, unit);

    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="size">文件大小</param>
    /// <param name="unit">文件大小单位</param>
    private static Int64 GetSize(Int64 size, FileSizeUnit unit)
    {
        switch (unit)
        {
            case FileSizeUnit.K:
                return size * 1024;

            case FileSizeUnit.M:
                return size * 1024 * 1024;

            case FileSizeUnit.G:
                return size * 1024 * 1024 * 1024;

            default:
                return size;
        }
    }

    /// <summary>
    /// 获取文件大小，单位：字节
    /// </summary>
    public readonly Int32 GetSize() => (Int32)Size;

    /// <summary>
    /// 获取文件大小，单位：K
    /// </summary>
    public readonly Double GetSizeByK() => Conv.ToDGDouble(Size / 1024.0, 2);

    /// <summary>
    /// 获取文件大小，单位：M
    /// </summary>
    public readonly Double GetSizeByM() => Conv.ToDGDouble(Size / 1024.0 / 1024.0, 2);

    /// <summary>
    /// 获取文件大小，单位：G
    /// </summary>
    public readonly Double GetSizeByG() => Conv.ToDGDouble(Size / 1024.0 / 1024.0 / 1024.0, 2);

    /// <summary>
    /// 输出描述
    /// </summary>
    public override readonly String ToString()
    {
        if (Size >= 1024 * 1024 * 1024)
            return $"{GetSizeByG()} {FileSizeUnit.G.Description()}";
        if (Size >= 1024 * 1024)
            return $"{GetSizeByM()} {FileSizeUnit.M.Description()}";
        if (Size >= 1024)
            return $"{GetSizeByK()} {FileSizeUnit.K.Description()}";
        return $"{GetSize()} {FileSizeUnit.Byte.Description()}";
    }
}