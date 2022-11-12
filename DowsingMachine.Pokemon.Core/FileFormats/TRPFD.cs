using FlatSharp.Attributes;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

[FlatBufferTable]
public class TRPFD
{
    [FlatBufferItem(0)] public ulong[] Hashes { get; set; }
    [FlatBufferItem(1)] public string[] Filenames { get; set; }
    [FlatBufferItem(2)] public TestSchemaUint[] Item3 { get; set; }
}

[FlatBufferTable]
public class TRPFD_3
{
    [FlatBufferItem(0)] public ulong Item1 { get; set; }
    [FlatBufferItem(1)] public TestSchemaUint Item2 { get; set; }
    public override string ToString() => $"{Item1},{Item2}";
}
