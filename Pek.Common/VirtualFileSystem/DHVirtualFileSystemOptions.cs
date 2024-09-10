﻿namespace Pek.VirtualFileSystem;

public class DHVirtualFileSystemOptions
{
    public VirtualFileSetList FileSets { get; }

    public DHVirtualFileSystemOptions()
    {
        FileSets = new VirtualFileSetList();
    }
}