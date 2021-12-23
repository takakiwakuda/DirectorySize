using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace DirectorySize;

/// <summary>
/// The Get-DirectorySize cmdlet gets the size of a directory.
/// </summary>
[Cmdlet(VerbsCommon.Get, "DirectorySize", DefaultParameterSetName = PathSet,
        HelpUri = "https://github.com/takakiwakuda/DirectorySize/blob/main/src/DirectorySize/doc/Get-DirectorySize.md")]
[OutputType(typeof(DirectorySizeInfo))]
public sealed class GetDirectorySizeCommand : PSCmdlet
{
    private const string PathSet = "Path";
    private const string LiteralPathSet = "LiteralPath";

    /// <summary>
    /// Gets or sets the Path parameter.
    /// </summary>
    [Parameter(ParameterSetName = PathSet, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    public string[]? Path
    {
        get
        {
            return _path;
        }
        set
        {
            _path = value;
        }
    }

    /// <summary>
    /// Gets or sets the LiteralPath parameter.
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = LiteralPathSet, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    [Alias("PSPath", "LP")]
    public string[]? LiteralPath
    {
        get
        {
            return _path;
        }
        set
        {
            _path = value;
            _isLiteralPath = true;
        }
    }

    /// <summary>
    /// Gets or sets the Recurse parameter.
    /// </summary>
    [Parameter]
    public SwitchParameter Recurse
    {
        get
        {
            return _recurse;
        }
        set
        {
            _recurse = value;
        }
    }

    private string[]? _path;
    private bool _isLiteralPath;
    private bool _recurse;

    /// <summary>
    /// Gets the size of a directory.
    /// </summary>
    protected override void ProcessRecord()
    {
        foreach (string path in GetResolvedFilePath())
        {
            GetDirectorySize(path);
        }
    }

    /// <summary>
    /// Gets the size of the specified directory and writes it to the pipe.
    /// </summary>
    /// <param name="path">The directory to get the size.</param>
    private void GetDirectorySize(string path)
    {
        DirectoryInfo directory = new(path);
        if (!directory.Exists || NativeMethods.IsReparsePoint(directory.FullName))
        {
            WriteDebug($"The path '{path}' is not a valid directory.");
            return;
        }

        int count = 0;
        long size = 0;

        if (_recurse)
        {
            Stack<DirectoryInfo> directories = new();
            directories.Push(directory);

            while (directories.Count != 0)
            {
                DirectoryInfo currentDirectory = directories.Pop();

                try
                {
                    foreach (FileSystemInfo item in currentDirectory.EnumerateFileSystemInfos())
                    {
                        if (item is DirectoryInfo directoryToPush)
                        {
                            if (NativeMethods.IsReparsePoint(directoryToPush.FullName))
                            {
                                WriteDebug($"The directory '{directoryToPush.FullName}' is a reparse point.");
                            }
                            else
                            {
                                directories.Push(directoryToPush);
                            }
                        }
                        else if (item is FileInfo file)
                        {
                            count++;
                            size += file.Length;
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    WriteWarning(ex.Message);
                }
            }
        }
        else
        {
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                count++;
                size += file.Length;
            }
        }

        WriteObject(new DirectorySizeInfo(directory, count, size));
    }

    /// <summary>
    /// Gets a fully qualified path from a path in the Path parameter.
    /// </summary>
    /// <returns>The fully qualified path of the <see cref="Path"/>.</returns>
    private string[] GetResolvedFilePath()
    {
        if (_path is null)
        {
            return new string[] { SessionState.Path.CurrentFileSystemLocation.ProviderPath };
        }

        List<string> filePaths = new();

        foreach (string path in _path)
        {
            try
            {
                if (!SessionState.InvokeProvider.Item.Exists(path, false, _isLiteralPath))
                {
                    ErrorRecord errorRecord = new(
                        new ItemNotFoundException($"Cannot find path '{path}' because it does not exist."),
                        "PathNotFound",
                        ErrorCategory.ObjectNotFound,
                        path
                    );
                    WriteError(errorRecord);
                    continue;
                }
            }
            catch (System.Management.Automation.DriveNotFoundException ex)
            {
                ErrorRecord errorRecord = new(ex.ErrorRecord, ex);
                WriteError(errorRecord);
                continue;
            }

            List<string> paths = new();
            ProviderInfo provider;

            if (_isLiteralPath)
            {
                paths.Add(SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out _));
            }
            else
            {
                paths.AddRange(SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider));
            }

            if (!provider.Name.Equals(FileSystemProvider.ProviderName, StringComparison.Ordinal))
            {
                ErrorRecord errorRecord = new(
                    new InvalidOperationException($"The path '{path}' is not a {FileSystemProvider.ProviderName}."),
                    "NotFileSystemProvider",
                    ErrorCategory.InvalidArgument,
                    path
                );
                WriteError(errorRecord);
                continue;
            }

            filePaths.AddRange(paths);
        }

        return filePaths.ToArray();
    }
}

/// <summary/>
public sealed class DirectorySizeInfo
{
    /// <summary/>
    public DirectoryInfo Directory { get; }

    /// <summary/>
    public int Files { get; }

    /// <summary/>
    public long Length { get; }

    /// <summary/>
    /// <param name="directory"/>
    /// <param name="files"/>
    /// <param name="length"/>
    /// <exception cref="ArgumentNullException"/>
    internal DirectorySizeInfo(DirectoryInfo directory, int files, long length)
    {
        Directory = directory ?? throw new ArgumentNullException(nameof(directory));
        Files = files;
        Length = length;
    }
}
