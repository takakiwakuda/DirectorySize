using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Security;
using Microsoft.PowerShell.Commands;

namespace DirectorySize;

/// <summary>
/// The Get-DirectorySize cmdlet retrieves the total size of files in a specified directory.
/// </summary>
[Cmdlet(VerbsCommon.Get, "DirectorySize", DefaultParameterSetName = PathSetName,
        HelpUri = "https://github.com/takakiwakuda/DirectorySize/blob/main/DirectorySize/docs/Get-DirectorySize.md")]
[OutputType(typeof(DirectorySizeInfo))]
public sealed class GetDirectorySizeCommand : PSCmdlet
{
    private const string PathSetName = "Path";
    private const string LiteralPathSetName = "LiteralPath";

    /// <summary>
    /// Gets or sets the Path parameter.
    /// </summary>
    [Parameter(ParameterSetName = PathSetName, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public string[]? Path
    {
        get
        {
            return _paths;
        }
        set
        {
            _isLiteralPath = false;
            _paths = value;
        }
    }

    /// <summary>
    /// Gets or sets the LiteralPath parameter.
    /// </summary>
    [Parameter(ParameterSetName = LiteralPathSetName, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    [Alias("PSPath", "LP")]
    public string[]? LiteralPath
    {
        get
        {
            return _paths;
        }
        set
        {
            _isLiteralPath = true;
            _paths = value;
        }
    }

    private bool _isLiteralPath;
    private string[]? _paths;

    /// <summary>
    /// ProcessRecord override.
    /// </summary>
    protected override void ProcessRecord()
    {
        HashSet<string> paths = new();

        if (_paths is null)
        {
            paths.Add(SessionState.Path.CurrentFileSystemLocation.ProviderPath);
        }
        else
        {
            paths.UnionWith(GetResolvedFilePathFromPSPath(_paths, _isLiteralPath));
        }

        DirectoryInfo directory;

        foreach (string path in paths)
        {
            directory = new(path);
            if (!directory.Exists)
            {
                WriteDebug(string.Format(StringResources.PathNotDirectory, path));
                continue;
            }

            WriteObject(GetDirectorySize(directory));
        }
    }

    /// <summary>
    /// Gets information about the size of the specified directory.
    /// </summary>
    /// <param name="directoryInfo">The directory to get the size.</param>
    /// <returns>Information about the size of the <paramref name="directoryInfo"/> parameter.</returns>
    private DirectorySizeInfo? GetDirectorySize(DirectoryInfo directoryInfo)
    {
        if (NativeMethodsHelper.IsReparsePoint(directoryInfo.FullName))
        {
            WriteDebug(string.Format(StringResources.PathIsReparsePoint, directoryInfo.FullName));
            return null;
        }

        Stack<DirectoryInfo> directories = new();
        directories.Push(directoryInfo);

        int totalFilesCount = 0;
        long totalSize = 0;

        DirectoryInfo currentDirectory;
        int filesCount;
        long size;

        while (directories.Count > 0)
        {
            currentDirectory = directories.Pop();
            filesCount = 0;
            size = 0;

            try
            {
                foreach (FileSystemInfo fileSystemEntry in currentDirectory.EnumerateFileSystemInfos())
                {
                    if (fileSystemEntry is DirectoryInfo directory)
                    {
                        directories.Push(directory);
                    }
                    else if (fileSystemEntry is FileInfo file)
                    {
                        filesCount++;
                        size += file.Length;
                    }
                }

                WriteDebug(string.Format(StringResources.DirectorySizeInfo, currentDirectory, filesCount, size));

                totalFilesCount += filesCount;
                totalSize += size;
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                WriteWarning(ex.Message);
            }
        }

        return new(directoryInfo, totalFilesCount, totalSize);
    }

    /// <summary>
    /// Resolves absolute file paths for specified paths.
    /// </summary>
    /// <param name="paths">Paths to resolve to absolute file paths.</param>
    /// <param name="isLiteralPath">A value that indicates whether <paramref name="paths"/> are literal paths.</param>
    /// <returns>Absolute file paths for <paramref name="paths"/>.</returns>
    private string[] GetResolvedFilePathFromPSPath(string[] paths, bool isLiteralPath)
    {
        List<string> filePaths = new();
        List<string> resolvedPaths = new();
        ProviderInfo provider;

        foreach (string path in paths)
        {
            if (!SessionState.InvokeProvider.Item.Exists(path, false, isLiteralPath))
            {
                ErrorRecord errorRecord = new(
                    new ItemNotFoundException(string.Format(StringResources.PathNotFound, path)),
                    "PathNotFound",
                    ErrorCategory.ObjectNotFound,
                    path);

                WriteError(errorRecord);
            }

            if (isLiteralPath)
            {
                resolvedPaths.Add(SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out _));
            }
            else
            {
                resolvedPaths.AddRange(SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider));
            }

            if (!provider.Name.Equals(FileSystemProvider.ProviderName, StringComparison.Ordinal))
            {
                ErrorRecord errorRecord = new(
                    new InvalidOperationException(string.Format(StringResources.NotFileSystemProvider, path)),
                    "NotFileSystemProvider",
                    ErrorCategory.InvalidArgument,
                    path);

                WriteError(errorRecord);
            }

            filePaths.AddRange(resolvedPaths);
            resolvedPaths.Clear();
        }

        return filePaths.ToArray();
    }
}

/// <summary>
/// Contains information about a directory and its size.
/// </summary>
public sealed class DirectorySizeInfo
{
    /// <summary>
    /// Gets information about the directory.
    /// </summary>
    public DirectoryInfo Directory { get; }

    /// <summary>
    /// Gets the number of files in the directory.
    /// </summary>
    public int FilesCount { get; }

    /// <summary>
    /// Gets the total size of files in the directory.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectorySizeInfo"/> class.
    /// </summary>
    /// <param name="directoryInfo">Information about the directory.</param>
    /// <param name="files">The number of files.</param>
    /// <param name="size">The total size of the directory.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="directoryInfo"/> is <see langword="null"/>.
    /// </exception>
    public DirectorySizeInfo(DirectoryInfo directoryInfo, int files, long size)
    {
        if (directoryInfo is null)
        {
            throw new ArgumentNullException(nameof(directoryInfo));
        }

        Directory = directoryInfo;
        FilesCount = files;
        Size = size;
    }
}
