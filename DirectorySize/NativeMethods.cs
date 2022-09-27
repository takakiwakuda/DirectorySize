using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DirectorySize;

internal static class NativeMethods
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct REPARSE_DATA_BUFFER
    {
        public uint ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved;
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 500)]
        public string PathBuffer;
    }

    public const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    public const int FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
    public const int FSCTL_GET_REPARSE_POINT = 0x000900A8;

    /// <summary>
    /// A value is one of the ReparseTag property representing junction points.
    /// </summary>
    public const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;

    /// <summary>
    /// A value is one of the ReparseTag property representing the symbolic link.
    /// </summary>
    public const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;

    /// <summary>
    /// The file or directory is not a reparse point.
    /// </summary>
    public const int STATUS_NOT_A_REPARSE_POINT = 0x1126;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern SafeFileHandle CreateFile(
        string lpFileName,
        int dwDesiredAccess,
        FileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        FileMode dwCreationDisposition,
        int dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        int dwIoControlCode,
        IntPtr lpInBuffer,
        int nInBufferSize,
        IntPtr lpOutBuffer,
        int nOutBufferSize,
        out int lpBytesReturned,
        IntPtr lpOverlapped);
}
