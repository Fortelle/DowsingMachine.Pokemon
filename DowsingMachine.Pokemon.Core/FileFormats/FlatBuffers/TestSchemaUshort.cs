using FlatSharp.Attributes;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

[FlatBufferTable]
public class TestSchemaUshort
{
    [FlatBufferItem(00)] public ushort A1 { get; set; }
    [FlatBufferItem(01)] public ushort A2 { get; set; }
    [FlatBufferItem(02)] public ushort A3 { get; set; }
    [FlatBufferItem(03)] public ushort A4 { get; set; }

    [FlatBufferItem(04)] public ushort B1 { get; set; }
    [FlatBufferItem(05)] public ushort B2 { get; set; }
    [FlatBufferItem(06)] public ushort B3 { get; set; }
    [FlatBufferItem(07)] public ushort B4 { get; set; }

    [FlatBufferItem(08)] public ushort C1 { get; set; }
    [FlatBufferItem(09)] public ushort C2 { get; set; }
    [FlatBufferItem(10)] public ushort C3 { get; set; }
    [FlatBufferItem(11)] public ushort C4 { get; set; }

    [FlatBufferItem(12)] public ushort D1 { get; set; }
    [FlatBufferItem(13)] public ushort D2 { get; set; }
    [FlatBufferItem(14)] public ushort D3 { get; set; }
    [FlatBufferItem(15)] public ushort D4 { get; set; }

    [FlatBufferItem(16)] public ushort E1 { get; set; }
    [FlatBufferItem(17)] public ushort E2 { get; set; }
    [FlatBufferItem(18)] public ushort E3 { get; set; }
    [FlatBufferItem(19)] public ushort E4 { get; set; }

    [FlatBufferItem(20)] public ushort F1 { get; set; }
    [FlatBufferItem(21)] public ushort F2 { get; set; }
    [FlatBufferItem(22)] public ushort F3 { get; set; }
    [FlatBufferItem(23)] public ushort F4 { get; set; }

    [FlatBufferItem(24)] public ushort G1 { get; set; }
    [FlatBufferItem(25)] public ushort G2 { get; set; }
    [FlatBufferItem(26)] public ushort G3 { get; set; }
    [FlatBufferItem(27)] public ushort G4 { get; set; }

    [FlatBufferItem(28)] public ushort H1 { get; set; }
    [FlatBufferItem(29)] public ushort H2 { get; set; }
    [FlatBufferItem(30)] public ushort H3 { get; set; }
    [FlatBufferItem(31)] public ushort H4 { get; set; }
}

