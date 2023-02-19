using FlatSharp.Attributes;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using PBT.DowsingMachine.Utilities.Codecs;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

[FlatBufferTable]
public class TRPFD
{
    [FlatBufferItem(0)] public ulong[] FileHashes { get; set; }
    [FlatBufferItem(1)] public string[] PackNames { get; set; }
    [FlatBufferItem(2)] public TRPFD_FileData[] FileData { get; set; }
    [FlatBufferItem(3)] public TRPFD_PackData[] PackData { get; set; }

    public string? FindPackName(string fileName)
    {
        var fileHash = FnvHash.Fnv1a_64(fileName);
        var fileIndex = Array.BinarySearch(FileHashes, fileHash);
        if (fileIndex < 0) return null;
        var packIndex = (int)FileData[fileIndex].Index;
        var packName = PackNames[packIndex];
        return packName;
    }

}

[FlatBufferTable]
public class TRPFD_FileData
{
    [FlatBufferItem(0)] public ulong Index { get; set; }
    [FlatBufferItem(1)] public TestSchemaUint Item2 { get; set; }
}

[FlatBufferTable]
public class TRPFD_PackData
{
    [FlatBufferItem(0)] public ulong Size { get; set; }
    [FlatBufferItem(1)] public ulong Length { get; set; }
}
