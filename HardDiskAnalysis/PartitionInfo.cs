namespace HardDiskAnalysis
{
	public class PartitionInfo
	{
		public string Letter { get; private set; }
		public string FileSystem { get; private set; }
		public ulong FreeSpace { get; private set; }
		public string Name { get; private set; }
		public string VolumeSerial { get; private set; }
		public ulong PartSize { get; private set; }

		public uint StartOffset { get; private set; }

		public PartitionInfo(string letter, string fileSystem, uint startOffset, ulong freeSpace, string name, string volumeSerial, ulong partSize)
		{
			Letter = letter;
			FileSystem = fileSystem;
			StartOffset = startOffset;
			FreeSpace = freeSpace;
			Name = name;
			VolumeSerial = volumeSerial;
			PartSize = partSize;
		}
	}
}
