using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace HardDiskAnalysis
{
	[Flags]
	public enum GenericFileAccess : uint
	{
		Read = 0x80000000,
		Write = 0x40000000,
		Execute = 0x20000000,
		All = 0x10000000
	}

	public enum CreationDisposition : uint
	{
		CreateNew = 1,
		CreateAlways = 2,
		OpenExisting = 3,
		OpenAlways = 4,
		TruncateExisting = 5
	}

	[Flags]
	public enum FileShareMode : uint
	{
		None = 0,
		Read = 1,
		Write = 2,
		Delete = 4
	}

	[Flags]
	public enum FlagsAndAttributes : uint
	{
		Overlapped = 0x40000000,
		Normal = 0x80,
		RandomAccess = 0x10000000
	}

	class Kernel32
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(string filename, GenericFileAccess desiredAccess, FileShareMode shareMode, uint securityAttributes, CreationDisposition creationDisposition,
													   FlagsAndAttributes flagsAndAttributes, IntPtr templateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(SafeFileHandle safeFileHandle);

		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		unsafe public static extern bool ReadFile(SafeFileHandle hFile, IntPtr outBuffer, int nOutBufferSize, IntPtr pBytesReturned, [In] NativeOverlapped* overlapped);

		[DllImport("Kernel32.dll", SetLastError = true)]
		unsafe public static extern bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, [In] ref NativeOverlapped lpOverlapped);

		[DllImport("Kernel32.dll", SetLastError = true)]
		unsafe public static extern bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, NativeOverlapped* lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		unsafe public static extern bool GetOverlappedResult(SafeFileHandle hFile, [In] ref NativeOverlapped lpOverlapped, out int lpNumberOfBytesTransferred, bool bWait);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		unsafe public static extern bool GetOverlappedResult(SafeFileHandle hFile, NativeOverlapped* lpOverlapped, out int lpNumberOfBytesTransferred, bool bWait);

	}
}
