using Microsoft.Extensions.FileProviders;

namespace Pek.VirtualFileSystem;

public class VirtualDirectoryFileInfo : IFileInfo
{
    public Boolean Exists => true;

    public Int64 Length => 0;

    public String PhysicalPath { get; }

    public String Name { get; }

    public DateTimeOffset LastModified { get; }

    public Boolean IsDirectory => true;

    public VirtualDirectoryFileInfo(String physicalPath, String name, DateTimeOffset lastModified)
    {
        PhysicalPath = physicalPath;
        Name = name;
        LastModified = lastModified;
    }

    public Stream CreateReadStream() => throw new InvalidOperationException();
}
