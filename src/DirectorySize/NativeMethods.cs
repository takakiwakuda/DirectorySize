using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DirectorySize;

internal static class NativeMethods
{
    private const int PathLength = 0x208;
    private const int STATUS_NOT_A_REPARSE_POINT = 0x1126;
    private const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    private const int FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
    private const int FSCTL_GET_REPARSE_POINT = 0x000900A8;
    private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
    private const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        string lpFileName,
        int dwDesiredAccess,
        FileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        FileMode dwCreationDisposition,
        int dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        int dwIoControlCode,
        IntPtr lpInBuffer,
        int nInBufferSize,
        IntPtr lpOutBuffer,
        int nOutBufferSize,
        out int lpBytesReturned,
        IntPtr lpOverlapped
    );

    [StructLayout(LayoutKind.Sequential)]
    private struct SymbolicLinkReparseBuffer
    {
        public uint ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved;
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength)] public byte[] PathBuffer;
    }

    internal static bool IsReparsePoint(string path)
    {
        using SafeFileHandle handle = CreateFile(
            path,
            0,
            FileShare.ReadWrite | FileShare.Delete,
            IntPtr.Zero,
            FileMode.Open,
            FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT,
            IntPtr.Zero
        );

        if (handle.IsInvalid)
        {
            return false;
        }

        SymbolicLinkReparseBuffer reparseBuffer;
        IntPtr buffer = IntPtr.Zero;
        int bufferSize = Marshal.SizeOf<SymbolicLinkReparseBuffer>();

        try
        {
            buffer = Marshal.AllocHGlobal(bufferSize);

            bool success = DeviceIoControl(
                handle,
                FSCTL_GET_REPARSE_POINT,
                IntPtr.Zero,
                0,
                buffer,
                bufferSize,
                out _,
                IntPtr.Zero
            );

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != STATUS_NOT_A_REPARSE_POINT)
                {
                    throw new Win32Exception(error);
                }

                return false;
            }

            reparseBuffer = Marshal.PtrToStructure<SymbolicLinkReparseBuffer>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return reparseBuffer.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT |
               reparseBuffer.ReparseTag == IO_REPARSE_TAG_SYMLINK;
    }
}
