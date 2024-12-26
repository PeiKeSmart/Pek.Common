using System.Reflection;

using Microsoft.Extensions.FileProviders;

using NewLife;

namespace Pek.VirtualFileSystem.Embedded;

public class EmbeddedFileSet : IVirtualFileSet
{
    public Assembly Assembly { get; }

    public String? BaseNamespace { get; }

    public String? BaseFolderInProject { get; }

    public EmbeddedFileSet(
        Assembly assembly,
        String? baseNamespace = null,
        String? baseFolderInProject = null)
    {
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        BaseNamespace = baseNamespace;
        BaseFolderInProject = baseFolderInProject;
    }

    public void AddFiles(Dictionary<String, IFileInfo> files)
    {
        var lastModificationTime = GetLastModificationTime();

        foreach (var resourcePath in Assembly.GetManifestResourceNames())
        {
            if (!BaseNamespace.IsNullOrEmpty() && !resourcePath.StartsWith(BaseNamespace))
            {
                continue;
            }

            var fullPath = ConvertToRelativePath(resourcePath).EnsureStartsWith('/');

            if (fullPath.Contains('/'))
            {
                AddDirectoriesRecursively(files, fullPath[..fullPath.LastIndexOf('/')], lastModificationTime);
            }

            files[fullPath] = new EmbeddedResourceFileInfo(
                Assembly,
                resourcePath,
                fullPath,
                CalculateFileName(fullPath),
                lastModificationTime
            );
        }
    }

    private static void AddDirectoriesRecursively(Dictionary<String, IFileInfo> files, String directoryPath, DateTimeOffset lastModificationTime)
    {
        if (files.ContainsKey(directoryPath))
        {
            return;
        }

        files[directoryPath] = new VirtualDirectoryFileInfo(
            directoryPath,
            CalculateFileName(directoryPath),
            lastModificationTime
        );

        if (directoryPath.Contains('/'))
        {
            AddDirectoriesRecursively(files, directoryPath[..directoryPath.LastIndexOf('/')], lastModificationTime);
        }
    }

    private DateTimeOffset GetLastModificationTime()
    {
        var lastModified = DateTimeOffset.UtcNow;

        if (!String.IsNullOrEmpty(Assembly.Location))
        {
            try
            {
                lastModified = File.GetLastWriteTimeUtc(Assembly.Location);
            }
            catch (PathTooLongException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return lastModified;
    }

    private String ConvertToRelativePath(String resourceName)
    {
        if (!BaseNamespace.IsNullOrEmpty())
        {
            resourceName = resourceName[(BaseNamespace.Length + 1)..];
        }

        var pathParts = resourceName.Split('.');
        if (pathParts.Length <= 2)
        {
            return resourceName;
        }

        var folder = pathParts.Take(pathParts.Length - 2).JoinAsString("/");
        var fileName = pathParts[^2] + "." + pathParts[^1];

        return folder + "/" + fileName;
    }

    private static String CalculateFileName(String filePath)
    {
        if (!filePath.Contains('/'))
        {
            return filePath;
        }

        return filePath.Substring(filePath.LastIndexOf("/", StringComparison.Ordinal) + 1);
    }
}