using FlatSharp.Attributes;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

[FlatBufferTable]
public class TRPFD
{
    [FlatBufferItem(0)] public TRPFD_1[] Item1 { get; set; }
    [FlatBufferItem(1)] public string[] Filenames { get; set; }
    [FlatBufferItem(2)] public TRPFD_3[] Item3 { get; set; }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct TRPFD_1
{
    [FieldOffset(00)] public uint Hash;
    [FieldOffset(04)] public uint Offset;

    public override string ToString() => $"{Hash:X8},{Offset}";
}

[FlatBufferTable]
public class TRPFD_3
{
    [FlatBufferItem(0)] public ulong Item1 { get; set; }
    [FlatBufferItem(1)] public uint Item2 { get; set; }
    [FlatBufferItem(2)] public ulong Item3 { get; set; }
    public override string ToString() => $"{Item1},{Item2},{Item3}";
}
