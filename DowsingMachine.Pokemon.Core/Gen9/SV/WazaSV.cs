using FlatSharp.Attributes;
using PBT.DowsingMachine.Projects;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.Gen9;

[FlatBufferTable]
public class WazaSV
{
    [FlatBufferItem(00)][StringReference(@"wazaname")] public ushort WazaNo { get; set; }
    [FlatBufferItem(01)] public bool Enable { get; set; }
    [FlatBufferItem(02)][StringReference(@"typename")] public byte Type { get; set; }
    [FlatBufferItem(03)] public byte Category { get; set; }
    [FlatBufferItem(04)] public byte DamageType { get; set; }
    [FlatBufferItem(05)] public byte Power { get; set; }
    [FlatBufferItem(06)] public byte HitPercent { get; set; }
    [FlatBufferItem(07)] public byte BasePP { get; set; }
    [FlatBufferItem(08)] public sbyte Priority { get; set; }
    [FlatBufferItem(09)] public byte HitCountMin { get; set; }

    [FlatBufferItem(10)] public byte HitCountMax { get; set; }
    [FlatBufferItem(11)] public WazaSVSick Sick { get; set; }
    [FlatBufferItem(12)] public byte CriticalRank { get; set; }
    [FlatBufferItem(13)] public byte ShrinkPer { get; set; }
    [FlatBufferItem(14)] public ushort AiSequenceNumber { get; set; }
    [FlatBufferItem(15)] public sbyte DamageRecoverRatio { get; set; }
    [FlatBufferItem(16)] public sbyte HpRecoverRatio { get; set; }
    [FlatBufferItem(17)] public byte Target { get; set; }
    [FlatBufferItem(18)] public WazaSVRankEffect RankEffect { get; set; }
    [FlatBufferItem(19)] public byte A { get; set; }

    [FlatBufferItem(20)] public bool Flag_Touch { get; set; }
    [FlatBufferItem(21)] public bool Flag_Tame { get; set; }
    [FlatBufferItem(22)] public bool Flag_Tire { get; set; }
    [FlatBufferItem(23)] public bool Flag_Mamoru { get; set; }
    [FlatBufferItem(24)] public bool Flag_MagicCoat { get; set; }
    [FlatBufferItem(25)] public bool Flag_Yokodori { get; set; }
    [FlatBufferItem(26)] public bool Flag_Oumugaeshi { get; set; }
    [FlatBufferItem(27)] public bool Flag_Punch { get; set; }
    [FlatBufferItem(28)] public bool Flag_Sound { get; set; }
    [FlatBufferItem(29)] public bool Flag_Dance { get; set; }

    [FlatBufferItem(30)] public bool Flag_Flying { get; set; }
    [FlatBufferItem(31)] public bool Flag_KooriMelt { get; set; }
    [FlatBufferItem(32)] public bool Flag_TripleFar { get; set; }
    [FlatBufferItem(33)] public bool Flag_KaifukuHuuji { get; set; }
    [FlatBufferItem(34)] public bool Flag_MigawariThru { get; set; }
    [FlatBufferItem(35)] public bool Flag_SkyBattleFail { get; set; }
    [FlatBufferItem(36)] public bool Flag_EffectToFriend { get; set; }
    [FlatBufferItem(37)] public bool Flag_Yubifuru { get; set; }
    [FlatBufferItem(38)] public bool Flag_EncoreFail { get; set; }
    [FlatBufferItem(39)] public bool Flag_SakidoriFail { get; set; }

    [FlatBufferItem(40)] public bool Flag_DelayAttack { get; set; }
    [FlatBufferItem(41)] public bool Flag_Pressure { get; set; }
    [FlatBufferItem(42)] public bool Flag_Combi { get; set; }
    [FlatBufferItem(43)] public bool Flag_NegotoOmit { get; set; }
    [FlatBufferItem(44)] public bool Flag_NekonoteOmit { get; set; }
    [FlatBufferItem(45)] public bool Flag_ManekkoOmit { get; set; }
    [FlatBufferItem(46)] public bool Flag_MonomaneFail { get; set; }
    [FlatBufferItem(47)] public bool Flag_SaihaiFail { get; set; }
    [FlatBufferItem(48)] public bool Flag_Boujin { get; set; }
    [FlatBufferItem(49)] public bool Flag_AgoBoost { get; set; }
    
    [FlatBufferItem(50)] public bool Flag_Boudan { get; set; }
    [FlatBufferItem(51)] public bool Flag_OyakoaiOmit { get; set; }
    [FlatBufferItem(52)] public bool Flag_NoAffinity { get; set; }
    [FlatBufferItem(53)] public bool Flag_Tikarazuku { get; set; }
    [FlatBufferItem(54)] public bool Flag_Kireaji { get; set; }
    [FlatBufferItem(55)] public bool Flag_Kaze { get; set; }
    [FlatBufferItem(56)] public bool Flag_HealingShift { get; set; }
    [FlatBufferItem(57)] public bool Flag_57 { get; set; }
    [FlatBufferItem(58)] public bool Flag_58 { get; set; }
    [FlatBufferItem(59)] public bool Flag_59 { get; set; }

    [FlatBufferItem(60)] public bool Flag_60 { get; set; }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct WazaSVSick
{
    [FieldOffset(0)] public ushort Id;
    [FieldOffset(2)] public byte Percent;
    [FieldOffset(3)] public byte Continuity;

    [FieldOffset(4)] public byte TurnMin;
    [FieldOffset(5)] public byte TurnMax;

    public override string ToString()
    {
        return $"{Id},{Percent},{Continuity},{TurnMin},{TurnMax}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct WazaSVRankEffect
{
    [FieldOffset(0)] public byte Type1;
    [FieldOffset(1)] public sbyte Value1;
    [FieldOffset(2)] public byte Percent1;

    [FieldOffset(3)] public byte Type2;
    [FieldOffset(4)] public sbyte Value2;
    [FieldOffset(5)] public byte Percent2;

    [FieldOffset(6)] public byte Type3;
    [FieldOffset(7)] public sbyte Value3;
    [FieldOffset(8)] public byte Percent3;

    public override string ToString()
    {
        return $"{Type1}/{Value1}/{Percent1}, {Type2}/{Value2}/{Percent2}, {Type3}/{Value3}/{Percent3}";
    }
}
