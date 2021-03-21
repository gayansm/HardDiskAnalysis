# Hard Disk Analysis

A set of methods and classes get information about hard disks, partitions and MBRs of a computer.

## Getting Started

Reference the project or dll in your project.

### Getting disk and partiton information.

```
var info = Disk.GetDriveInfo();
```

### Get MBR values

Some of the values of the MBR class are in raw binary format. May need some work to parse them.

```
var mbr = Mbr.ReadMbr();
```

### Writing to disks
Writes can only be done at sector start addresses. Length of data to be written must be a multiple of sector size. The disks are opened with NORMAL parameter which is supposed to enable bufferring and support writing data with arbitrary length to arbitrary addresses, but this did not work during my tests.

```
var offset = 2048;
using var handle = Disk.OpenDiskForWrites(diskPhysicalId);
using var fs = Disk.InitializeFileStreamToWriteToDisk(handle, offset);
var buff = new byte[512];
// Writes can be done directly using the FileStream or using the following method.
Disk.WriteToDisk(fs, buff);
buff = new byte[2048];
// New data will be written after the previous data since the offset increases as you write to the streaming.
Disk.WriteToDisk(fs, buff);
```

## Authors

* **Gayan Mudalige** - *Initial work* - (https://gist.github.com/gayansm)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

