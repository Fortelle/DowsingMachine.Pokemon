using FlatSharp.Attributes;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

[FlatBufferTable]
public class TestSchemaUlong
{
    [FlatBufferItem(00)] public TestSchemaItem A1 { get; set; }
    [FlatBufferItem(01)] public TestSchemaItem A2 { get; set; }
    [FlatBufferItem(02)] public TestSchemaItem A3 { get; set; }
    [FlatBufferItem(03)] public TestSchemaItem A4 { get; set; }

    [FlatBufferItem(04)] public TestSchemaItem B1 { get; set; }
    [FlatBufferItem(05)] public TestSchemaItem B2 { get; set; }
    [FlatBufferItem(06)] public TestSchemaItem B3 { get; set; }
    [FlatBufferItem(07)] public TestSchemaItem B4 { get; set; }

    [FlatBufferItem(08)] public TestSchemaItem C1 { get; set; }
    [FlatBufferItem(09)] public TestSchemaItem C2 { get; set; }
    [FlatBufferItem(10)] public TestSchemaItem C3 { get; set; }
    [FlatBufferItem(11)] public TestSchemaItem C4 { get; set; }

    [FlatBufferItem(12)] public TestSchemaItem D1 { get; set; }
    [FlatBufferItem(13)] public TestSchemaItem D2 { get; set; }
    [FlatBufferItem(14)] public TestSchemaItem D3 { get; set; }
    [FlatBufferItem(15)] public TestSchemaItem D4 { get; set; }

    [FlatBufferItem(16)] public TestSchemaItem E1 { get; set; }
    [FlatBufferItem(17)] public TestSchemaItem E2 { get; set; }
    [FlatBufferItem(18)] public TestSchemaItem E3 { get; set; }
    [FlatBufferItem(19)] public TestSchemaItem E4 { get; set; }

    [FlatBufferItem(20)] public TestSchemaItem F1 { get; set; }
    [FlatBufferItem(21)] public TestSchemaItem F2 { get; set; }
    [FlatBufferItem(22)] public TestSchemaItem F3 { get; set; }
    [FlatBufferItem(23)] public TestSchemaItem F4 { get; set; }

    [FlatBufferItem(24)] public TestSchemaItem G1 { get; set; }
    [FlatBufferItem(25)] public TestSchemaItem G2 { get; set; }
    [FlatBufferItem(26)] public TestSchemaItem G3 { get; set; }
    [FlatBufferItem(27)] public TestSchemaItem G4 { get; set; }

    [FlatBufferItem(28)] public TestSchemaItem H1 { get; set; }
    [FlatBufferItem(29)] public TestSchemaItem H2 { get; set; }
    [FlatBufferItem(30)] public TestSchemaItem H3 { get; set; }
    [FlatBufferItem(31)] public TestSchemaItem H4 { get; set; }
}

