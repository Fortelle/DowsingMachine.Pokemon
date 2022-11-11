using FlatSharp.Attributes;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

[FlatBufferTable]
public class TestSchemaString
{
    [FlatBufferItem(00)] public string A1 { get; set; }
    [FlatBufferItem(01)] public string A2 { get; set; }
}

