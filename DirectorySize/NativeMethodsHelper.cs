using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using static DirectorySize.NativeMethods;

namespace DirectorySize;

internal static class NativeMethodsHelper
{
    /// <summary>
    /// Gets a value that indicates whether the specified file or directory is a reparse point.
    /// </summary>
    /// <param name="path">The path to the file or directory to test.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="path"/> parameter is a reparse point;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsReparsePoint(string path)
    {
        using SafeFileHandle handle = CreateFile(
            path,
            0,
            FileShare.ReadWrite | FileShare.Delete,
            IntPtr.Zero,
            FileMode.Open,
            FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT,
            IntPtr.Zero);

        if (handle.IsInvalid)
        {
            return false;
        }

        int size = Marshal.SizeOf<REPARSE_DATA_BUFFER>();
        IntPtr ptr = IntPtr.Zero;
        REPARSE_DATA_BUFFER buffer;

        try
        {
            ptr = Marshal.AllocHGlobal(size);

            if (!DeviceIoControl(handle, FSCTL_GET_REPARSE_POINT, IntPtr.Zero, 0, ptr, size, out _, IntPtr.Zero))
            {
                return false;
            }

            buffer = Marshal.PtrToStructure<REPARSE_DATA_BUFFER>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return buffer.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT || buffer.ReparseTag == IO_REPARSE_TAG_SYMLINK;
    }
}
