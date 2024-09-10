namespace Pek.VirtualFileSystem;

public class VirtualFileSetList : List<IVirtualFileSet>
{
    public List<string> PhysicalPaths { get; }

    public VirtualFileSetList()
    {
        PhysicalPaths = new List<string>();
    }
}