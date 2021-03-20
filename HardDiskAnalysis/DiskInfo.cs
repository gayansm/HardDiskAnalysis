namespace HardDiskAnalysis
{
	public class DiskInfo
	{
		public string PhysicalName { get; private set; }
		public string DeviceId { get; private set; }
		public string Model { get; private set; }
		public string Interface { get; private set; }
		public bool MediaLoaded { get; private set; }
		public bool MediaOk { get; private set; }
		public uint SectorSize { get; private set; }
		public PartitionInfo[] PartitionInfo { get; set; }

		public DiskInfo(string physicalName, string deviceId, string model, string @interface, bool loaded, bool ok, uint sectorSize)
		{
			PhysicalName = physicalName;
			DeviceId = deviceId;
			Model = model;
			Interface = @interface;
			MediaLoaded = loaded;
			MediaOk = ok;
			SectorSize = sectorSize;
			PartitionInfo = new PartitionInfo[0];
		}
	}
}
