using System;
using System.Collections.Generic;
using System.IO;

namespace HardDiskAnalysis
{
	public class Mbr
	{
		public byte[] Legacy = new byte[0x1B8];
		public uint Sig = 0;
		public byte[] Reserved = new byte[2];
		List<PartitionEntry> Partitions = new List<PartitionEntry>();
		public ushort Magic = 0;

		public static Mbr ReadMbr(string physicalDiskId)
		{
			var handle = Disk.OpenDiskForReads(physicalDiskId);

			var len = 512;

			using var fs = new FileStream(handle, FileAccess.Read, len, false);
			fs.Seek(0, SeekOrigin.Begin);
			var buff = new byte[len];
			var bytesRead = fs.Read(buff, 0, buff.Length);

			if (bytesRead != len)
				throw new Exception("Failed to read MBR");

			return Parse(buff);
		}

		public static Mbr Parse(byte[] rawMbr)
		{
			if (rawMbr.Length != 512)
				throw new Exception("Invalid mbr buffer length");

			var mbr = new Mbr();
			using var s = new MemoryStream(rawMbr);
			using var r = new BinaryReader(s);
			mbr.Legacy = r.ReadBytes(mbr.Legacy.Length);
			mbr.Sig = r.ReadUInt32();
			mbr.Reserved = r.ReadBytes(mbr.Reserved.Length);

			for (var i = 0; i < 4; i ++)
			{
				var bytes = r.ReadBytes(16);
				mbr.Partitions.Add(PartitionEntry.Parse(bytes));
			}

			mbr.Magic = r.ReadUInt16();
			return mbr;
		}
	}

	public class PartitionEntry
	{
		public byte PartStatus;
		public byte StartHead;
		public byte StartSector; // 6
		public byte StartTrackMsb; //2
		public byte StartTrackLsb;
		public byte Type;
		public byte EndHead;
		public byte EndSector; //6
		public byte EndTrackMsb; //2
		public byte EndTrackLsb;
		public uint Lba;
		public uint NumberOfSectors;

		public static PartitionEntry Parse(byte[] entry)
		{
			if (entry.Length != 16)
				throw new Exception("Buffer length invalid.");

			using var ms = new MemoryStream(entry);
			using var r = new BinaryReader(ms);
			var part = new PartitionEntry();
			part.PartStatus = r.ReadByte();
			part.StartHead = r.ReadByte();
			var temp = r.ReadByte();
			part.StartSector = GetBitValue(temp, 0, 6);
			part.StartTrackMsb = GetBitValue(temp, 6, 2);
			part.StartTrackLsb = r.ReadByte();
			part.Type = r.ReadByte();
			part.EndHead = r.ReadByte();
			temp = r.ReadByte();
			part.EndSector = GetBitValue(temp, 0, 6);
			part.EndTrackMsb = GetBitValue(temp, 6, 2);
			part.EndTrackLsb = r.ReadByte();
			part.Lba = r.ReadUInt32();
			part.NumberOfSectors = r.ReadUInt32();

			return part;
		}

		private static Status ParseStatus(byte bytes)
		{
			if (bytes == 0x00)
				return Status.Inactive;
			if (bytes == 0x80)
				return Status.Bootable;

			return Status.Invalid;
		}
		public enum Status
		{
			Invalid = 0,
			Inactive,
			Bootable
		}

		private static byte GetBitValue(byte comp, int startIndex, int length)
		{
			comp = (byte)(comp >> startIndex);
			var pow = (byte)(Math.Pow(2, length) - 1);
			var val = (byte)(comp & pow);
			return val;
		}
	}
}
