using FlatSharp.Attributes;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

[FlatBufferTable]
public class TestSchemaUint
{
    [FlatBufferItem(00)] public uint A1 { get; set; }
    [FlatBufferItem(01)] public uint A2 { get; set; }
    [FlatBufferItem(02)] public uint A3 { get; set; }
    [FlatBufferItem(03)] public uint A4 { get; set; }

    [FlatBufferItem(04)] public uint B1 { get; set; }
    [FlatBufferItem(05)] public uint B2 { get; set; }
    [FlatBufferItem(06)] public uint B3 { get; set; }
    [FlatBufferItem(07)] public uint B4 { get; set; }

    [FlatBufferItem(08)] public uint C1 { get; set; }
    [FlatBufferItem(09)] public uint C2 { get; set; }
    [FlatBufferItem(10)] public uint C3 { get; set; }
    [FlatBufferItem(11)] public uint C4 { get; set; }

    [FlatBufferItem(12)] public uint D1 { get; set; }
    [FlatBufferItem(13)] public uint D2 { get; set; }
    [FlatBufferItem(14)] public uint D3 { get; set; }
    [FlatBufferItem(15)] public uint D4 { get; set; }

    [FlatBufferItem(16)] public uint E1 { get; set; }
    [FlatBufferItem(17)] public uint E2 { get; set; }
    [FlatBufferItem(18)] public uint E3 { get; set; }
    [FlatBufferItem(19)] public uint E4 { get; set; }

    [FlatBufferItem(20)] public uint F1 { get; set; }
    [FlatBufferItem(21)] public uint F2 { get; set; }
    [FlatBufferItem(22)] public uint F3 { get; set; }
    [FlatBufferItem(23)] public uint F4 { get; set; }

    [FlatBufferItem(24)] public uint G1 { get; set; }
    [FlatBufferItem(25)] public uint G2 { get; set; }
    [FlatBufferItem(26)] public uint G3 { get; set; }
    [FlatBufferItem(27)] public uint G4 { get; set; }

    [FlatBufferItem(28)] public uint H1 { get; set; }
    [FlatBufferItem(29)] public uint H2 { get; set; }
    [FlatBufferItem(30)] public uint H3 { get; set; }
    [FlatBufferItem(31)] public uint H4 { get; set; }

    public override string ToString() => $"{A1},{A2},{A3},{A4}; {B1},{B2},{B3},{B4}; {C1},{C2},{C3},{C4}; {D1},{D2},{D3},{D4}";
}

