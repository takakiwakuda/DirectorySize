namespace DirectorySize;

internal static class StringResources
{
    internal const string DirectorySizeInfo = "{0}: files = {1}, size = {2}";
    internal const string NotFileSystemProvider = "The path '{0}' is not a filesystem.";
    internal const string PathIsReparsePoint = "Ignores the path '{0}' because it is a reparse point.";
    internal const string PathNotDirectory = "Ignores the path '{0}' because it is not a directory.";
    internal const string PathNotFound = "Cannot find path '{0}' because it does not exist.";
}
