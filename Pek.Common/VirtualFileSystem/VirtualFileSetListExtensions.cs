using NewLife;

using Pek.VirtualFileSystem.Embedded;

namespace Pek.VirtualFileSystem;

public static class VirtualFileSetListExtensions
{
    public static void AddEmbedded<T>(this VirtualFileSetList list, String? baseNamespace = null, String? baseFolderInProject = null)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));

        list.Add(
            new EmbeddedFileSet(
                typeof(T).Assembly,
                baseNamespace,
                baseFolderInProject
            )
        );
    }

    public static void ReplaceEmbeddedByPhysical<T>(this VirtualFileSetList list, string pyhsicalPath)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (pyhsicalPath == null) throw new ArgumentNullException(nameof(pyhsicalPath));

        var assembly = typeof(T).Assembly;
        var embeddedFileSets = list.OfType<EmbeddedFileSet>().Where(fs => fs.Assembly == assembly).ToList();

        foreach (var embeddedFileSet in embeddedFileSets)
        {
            list.Remove(embeddedFileSet);

            if (!embeddedFileSet.BaseFolderInProject.IsNullOrEmpty())
            {
                pyhsicalPath = Path.Combine(pyhsicalPath, embeddedFileSet.BaseFolderInProject);
            }

            list.PhysicalPaths.Add(pyhsicalPath);
        }
    }
}
