using FlatSharp.Attributes;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct TestSchemaItem
{
    [FieldOffset(00)] public ulong Value;

    public override string ToString()
    {
        return Value == 0 ? "" : $"0x{Value:X16}";
    }
}

