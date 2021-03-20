namespace HardDiskAnalysis
{
    public class PartitionEntryLite
    {
        public uint LbaStart { get; private set; }

        public uint NumSectors { get; private set; }

        public ulong Size { get; set; }

        public int Type { get; private set; }

        public Mbr.Status PartitionStatus { get; private set; }

        public PartitionEntryLite(uint lbaStart, uint numSectors, int @type, Mbr.Status partitionStatus, ulong size)
        {
            LbaStart = lbaStart;
            NumSectors = numSectors;
            Size = size;
            Type = @type;
            PartitionStatus = partitionStatus;
        }
    }
}
