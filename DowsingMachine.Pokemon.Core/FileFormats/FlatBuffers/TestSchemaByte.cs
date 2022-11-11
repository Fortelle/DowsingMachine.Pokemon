using FlatSharp.Attributes;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

[FlatBufferTable]
public class TestSchemaByte
{
    [FlatBufferItem(00)] public byte A1 { get; set; }
    [FlatBufferItem(01)] public byte A2 { get; set; }
    [FlatBufferItem(02)] public byte A3 { get; set; }
    [FlatBufferItem(03)] public byte A4 { get; set; }

    [FlatBufferItem(04)] public byte B1 { get; set; }
    [FlatBufferItem(05)] public byte B2 { get; set; }
    [FlatBufferItem(06)] public byte B3 { get; set; }
    [FlatBufferItem(07)] public byte B4 { get; set; }

    [FlatBufferItem(08)] public byte C1 { get; set; }
    [FlatBufferItem(09)] public byte C2 { get; set; }
    [FlatBufferItem(10)] public byte C3 { get; set; }
    [FlatBufferItem(11)] public byte C4 { get; set; }

    [FlatBufferItem(12)] public byte D1 { get; set; }
    [FlatBufferItem(13)] public byte D2 { get; set; }
    [FlatBufferItem(14)] public byte D3 { get; set; }
    [FlatBufferItem(15)] public byte D4 { get; set; }

    [FlatBufferItem(16)] public byte E1 { get; set; }
    [FlatBufferItem(17)] public byte E2 { get; set; }
    [FlatBufferItem(18)] public byte E3 { get; set; }
    [FlatBufferItem(19)] public byte E4 { get; set; }

    [FlatBufferItem(20)] public byte F1 { get; set; }
    [FlatBufferItem(21)] public byte F2 { get; set; }
    [FlatBufferItem(22)] public byte F3 { get; set; }
    [FlatBufferItem(23)] public byte F4 { get; set; }

    [FlatBufferItem(24)] public byte G1 { get; set; }
    [FlatBufferItem(25)] public byte G2 { get; set; }
    [FlatBufferItem(26)] public byte G3 { get; set; }
    [FlatBufferItem(27)] public byte G4 { get; set; }

    [FlatBufferItem(28)] public byte H1 { get; set; }
    [FlatBufferItem(29)] public byte H2 { get; set; }
    [FlatBufferItem(30)] public byte H3 { get; set; }
    [FlatBufferItem(31)] public byte H4 { get; set; }
}

