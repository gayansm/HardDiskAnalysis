using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace HardDiskAnalysis
{
	public static class Disk
	{
		private static DiskInfo[] lastDiskInfo;

		public static DiskInfo[] GetDriveInfo()
		{
#pragma warning disable CA1416 // Validate platform compatibility
			if (lastDiskInfo != null)
				return lastDiskInfo;

			var driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive");
			var diskInfo = new List<DiskInfo>();

			foreach (ManagementObject d in driveQuery.Get())
			{
				var deviceId = Convert.ToString(d.Properties["DeviceId"].Value);
				var physicalName = Convert.ToString(d.Properties["Name"].Value); // \\.\PHYSICALDRIVE2
				var diskName = Convert.ToString(d.Properties["Caption"].Value); // WDC WD5001AALS-xxxxxx
				var diskModel = Convert.ToString(d.Properties["Model"].Value); // WDC WD5001AALS-xxxxxx
				var diskInterface = Convert.ToString(d.Properties["InterfaceType"].Value); // IDE
				var capabilities = (ushort[])d.Properties["Capabilities"].Value; // 3,4 - random access, supports writing
				var mediaLoaded = Convert.ToBoolean(d.Properties["MediaLoaded"].Value); // bool
				var mediaType = Convert.ToString(d.Properties["MediaType"].Value); // Fixed hard disk media
				var mediaSignature = Convert.ToUInt32(d.Properties["Signature"].Value); // int32
				var mediaStatus = Convert.ToString(d.Properties["Status"].Value) == "OK"; // OK
				var sectorSize = Convert.ToUInt32(d.Properties["BytesPerSector"].Value); // 512

				var currentDisk = new DiskInfo(physicalName, deviceId, diskModel, diskInterface, mediaLoaded, mediaStatus, sectorSize);
				diskInfo.Add(currentDisk);

				var partitionQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_DiskDriveToDiskPartition", d.Path.RelativePath);
				var partitionQuery = new ManagementObjectSearcher(partitionQueryText);

				var partList = new List<PartitionInfo>();

				foreach (ManagementObject p in partitionQuery.Get())
				{
					var startOffset = Convert.ToUInt64(p.Properties["StartingOffset"].Value);
					var index = Convert.ToUInt32(p.Properties["Index"].Value);
					var size = Convert.ToUInt64(p.Properties["Size"].Value);
					var name = Convert.ToString(p.Properties["Name"].Value);
					var bootPartition = Convert.ToBoolean(p.Properties["BootPartition"].Value);
					var bootable = Convert.ToBoolean(p.Properties["Bootable"].Value);
					var blockSize = Convert.ToUInt64(p.Properties["BlockSize"].Value);

					var logicalDriveQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_LogicalDiskToPartition", p.Path.RelativePath);
					var logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);

					foreach (ManagementObject ld in logicalDriveQuery.Get())
					{
						// Original disk info

						var driveName = Convert.ToString(ld.Properties["Name"].Value); // C:
						var driveId = Convert.ToString(ld.Properties["DeviceId"].Value); // C:
						var driveCompressed = Convert.ToBoolean(ld.Properties["Compressed"].Value);
						var driveType = Convert.ToUInt32(ld.Properties["DriveType"].Value); // C: - 3
						var fileSystem = Convert.ToString(ld.Properties["FileSystem"].Value); // NTFS
						var freeSpace = Convert.ToUInt64(ld.Properties["FreeSpace"].Value); // in bytes
						var totalSpace = Convert.ToUInt64(ld.Properties["Size"].Value); // in bytes
						var driveMediaType = Convert.ToUInt32(ld.Properties["MediaType"].Value); // c: 12
						var volumeName = Convert.ToString(ld.Properties["VolumeName"].Value); // System
						var volumeSerial = Convert.ToString(ld.Properties["VolumeSerialNumber"].Value); // 12345678

						partList.Add(new PartitionInfo(driveName, fileSystem, (uint)startOffset, freeSpace, volumeName, volumeSerial, size));

#pragma warning restore CA1416 // Validate platform compatibility
					}
				}

				currentDisk.PartitionInfo = partList.ToArray();
			}
			lastDiskInfo = diskInfo.ToArray();
			return lastDiskInfo;
		}

		public static bool Exists(string diskPhysicalId)
		{
			return GetDriveInfo().Any(x => x.PhysicalName == diskPhysicalId);
		}

		public static string GetOsDiskId()
		{
			var info = GetDriveInfo();
			var osLetter = Path.GetPathRoot(Environment.GetFolderPath(
				Environment.SpecialFolder.System)).TrimEnd('\\');
			var osDisk = info.First(x => x.PartitionInfo.Any(y => y.Letter == osLetter));
			return osDisk?.PhysicalName ?? string.Empty;
		}

		public static SafeFileHandle OpenDiskForWrites(string diskPhysicalId)
		{
			var handle = Kernel32.CreateFile(diskPhysicalId,
			   GenericFileAccess.Read | GenericFileAccess.Write,
			   FileShareMode.Read | FileShareMode.Write,
			   0,
			   CreationDisposition.OpenExisting,
			   FlagsAndAttributes.Normal | FlagsAndAttributes.Overlapped | FlagsAndAttributes.RandomAccess,
			   IntPtr.Zero);

			if (handle.IsInvalid)
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

			return handle;
		}

		public static SafeFileHandle OpenDiskForReads(string diskPhysicalId)
		{
			var handle = Kernel32.CreateFile(diskPhysicalId,
			   GenericFileAccess.Read,
			   FileShareMode.Read | FileShareMode.Write,
			   0, CreationDisposition.OpenExisting,
			   0, IntPtr.Zero);

			if (handle.IsInvalid)
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

			return handle;
		}

		public static FileStream InitialiseFileStreamToWriteToDisk(SafeFileHandle handle, uint offset)
		{
			var fs = new FileStream(handle, FileAccess.Read | FileAccess.Write, 4096, true);
			fs.Seek(offset, SeekOrigin.Begin);
			return fs;
		}

		/// <summary>
		/// For the first write, set seekToOffset to true and pass in the offset. This is not required for subsequent writes to the stream.
		/// </summary>
		/// <param name="fs"></param>
		/// <param name="length"></param>
		/// <param name="data"></param>
		/// <param name="seekToOffsetFromBeginningOfFile"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static void WriteToDisk(FileStream fs, byte[] data, bool seekToOffsetFromBeginningOfFile = false, uint offset = 0)
		{
			if (seekToOffsetFromBeginningOfFile)
				fs.Seek(offset, SeekOrigin.Begin);

			fs.Write(data, 0, data.Length);
		}
	}
}
