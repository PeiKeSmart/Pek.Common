using Microsoft.Extensions.FileProviders;

namespace Pek.VirtualFileSystem;

public interface IVirtualFileSet
{
    void AddFiles(Dictionary<string, IFileInfo> files);
}