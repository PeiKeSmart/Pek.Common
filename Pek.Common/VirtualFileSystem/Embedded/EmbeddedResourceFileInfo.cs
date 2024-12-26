using System.Reflection;

using Microsoft.Extensions.FileProviders;

namespace Pek.VirtualFileSystem.Embedded;

/// <summary>
/// 表示嵌入到程序集中的文件。
/// </summary>
public class EmbeddedResourceFileInfo : IFileInfo
{
    public Boolean Exists => true;

    public Int64 Length
    {
        get
        {
            if (!_length.HasValue)
            {
                using var stream = _assembly.GetManifestResourceStream(_resourcePath);
                _length = stream?.Length;
            }

            return _length!.Value;
        }
    }
    private Int64? _length;

    public String? PhysicalPath => null;

    public String VirtualPath { get; }

    public String Name { get; }

    /// <summary>
    /// 时间，以UTC为单位。
    /// </summary>
    public DateTimeOffset LastModified { get; }

    public Boolean IsDirectory => false;

    private readonly Assembly _assembly;
    private readonly String _resourcePath;

    public EmbeddedResourceFileInfo(
        Assembly assembly,
        String resourcePath,
        String virtualPath,
        String name,
        DateTimeOffset lastModified)
    {
        _assembly = assembly;
        _resourcePath = resourcePath;

        VirtualPath = virtualPath;
        Name = name;
        LastModified = lastModified;
    }

    /// <inheritdoc />
    public Stream CreateReadStream()
    {
        var stream = _assembly.GetManifestResourceStream(_resourcePath);

        if (!_length.HasValue && stream != null)
        {
            _length = stream.Length;
        }

        return stream!;
    }

    public override String ToString()
    {
        return $"[EmbeddedResourceFileInfo] {Name} ({VirtualPath})";
    }
}