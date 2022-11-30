using FlatSharp.Attributes;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

[FlatBufferTable]
public class TRPFD
{
    [FlatBufferItem(0)] public ulong[] UnpackedFileHashes { get; set; }
    [FlatBufferItem(1)] public string[] PackFilenames { get; set; }
    [FlatBufferItem(2)] public TRPFD_UnpackedFileData[] UnpackedFileData { get; set; }
    [FlatBufferItem(3)] public TRPFD_PackFileData[] PackFileData { get; set; }
}

[FlatBufferTable]
public class TRPFD_UnpackedFileData
{
    [FlatBufferItem(0)] public ulong Index { get; set; }
    [FlatBufferItem(1)] public TestSchemaUint Item2 { get; set; }
}

[FlatBufferTable]
public class TRPFD_PackFileData
{
    [FlatBufferItem(0)] public ulong Size { get; set; }
    [FlatBufferItem(1)] public ulong Length { get; set; }
}
