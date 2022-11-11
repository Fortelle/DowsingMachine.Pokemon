using FlatSharp.Attributes;
using System.ComponentModel;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

[FlatBufferTable]
public class Waza8
{
    [FlatBufferItem(00)] public uint Version { get; set; }
    [FlatBufferItem(01)] public uint MoveID { get; set; }
    [FlatBufferItem(02)] public bool CanUseMove { get; set; }
    [FlatBufferItem(03), Browsable(false)] public byte FType { get; set; }
    [FlatBufferItem(04), Browsable(false)] public byte FQuality { get; set; }
    [FlatBufferItem(05), Browsable(false)] public byte FCategory { get; set; }
    [FlatBufferItem(06), Browsable(false)] public byte FPower { get; set; }
    [FlatBufferItem(07), Browsable(false)] public byte FAccuracy { get; set; }
    [FlatBufferItem(08), Browsable(false)] public byte FPP { get; set; }
    [FlatBufferItem(09), Browsable(false)] public byte FPriority { get; set; }
    [FlatBufferItem(10), Browsable(false)] public byte FHitMin { get; set; }
    [FlatBufferItem(11), Browsable(false)] public byte FHitMax { get; set; }
    [FlatBufferItem(12), Browsable(false)] public ushort FInflict { get; set; }
    [FlatBufferItem(13), Browsable(false)] public byte FInflictPercent { get; set; }
    [FlatBufferItem(14), Browsable(false)] public byte FRawInflictCount { get; set; }
    [FlatBufferItem(15), Browsable(false)] public byte FTurnMin { get; set; }
    [FlatBufferItem(16), Browsable(false)] public byte FTurnMax { get; set; }
    [FlatBufferItem(17), Browsable(false)] public byte FCritStage { get; set; }
    [FlatBufferItem(18), Browsable(false)] public byte FFlinch { get; set; }
    [FlatBufferItem(19), Browsable(false)] public ushort FEffectSequence { get; set; }
    [FlatBufferItem(20), Browsable(false)] public byte FRecoil { get; set; }
    [FlatBufferItem(21), Browsable(false)] public byte FRawHealing { get; set; }
    [FlatBufferItem(22), Browsable(false)] public byte FRawTarget { get; set; }
    [FlatBufferItem(23), Browsable(false)] public byte FStat1 { get; set; }
    [FlatBufferItem(24), Browsable(false)] public byte FStat2 { get; set; }
    [FlatBufferItem(25), Browsable(false)] public byte FStat3 { get; set; }
    [FlatBufferItem(26), Browsable(false)] public byte FStat1Stage { get; set; }
    [FlatBufferItem(27), Browsable(false)] public byte FStat2Stage { get; set; }
    [FlatBufferItem(28), Browsable(false)] public byte FStat3Stage { get; set; }
    [FlatBufferItem(29), Browsable(false)] public byte FStat1Percent { get; set; }
    [FlatBufferItem(30), Browsable(false)] public byte FStat2Percent { get; set; }
    [FlatBufferItem(31), Browsable(false)] public byte FStat3Percent { get; set; }
    [FlatBufferItem(32)] public byte GigantamaxPower { get; set; }
    [FlatBufferItem(33)] public bool Flag_MakesContact { get; set; }
    [FlatBufferItem(34)] public bool Flag_Charge { get; set; }
    [FlatBufferItem(35)] public bool Flag_Recharge { get; set; }
    [FlatBufferItem(36)] public bool Flag_Protect { get; set; }
    [FlatBufferItem(37)] public bool Flag_Reflectable { get; set; }
    [FlatBufferItem(38)] public bool Flag_Snatch { get; set; }
    [FlatBufferItem(39)] public bool Flag_Mirror { get; set; }
    [FlatBufferItem(40)] public bool Flag_Punch { get; set; }
    [FlatBufferItem(41)] public bool Flag_Sound { get; set; }
    [FlatBufferItem(42)] public bool Flag_Gravity { get; set; }
    [FlatBufferItem(43)] public bool Flag_Defrost { get; set; }
    [FlatBufferItem(44)] public bool Flag_DistanceTriple { get; set; }
    [FlatBufferItem(45)] public bool Flag_Heal { get; set; }
    [FlatBufferItem(46)] public bool Flag_IgnoreSubstitute { get; set; }
    [FlatBufferItem(47)] public bool Flag_FailSkyBattle { get; set; }
    [FlatBufferItem(48)] public bool Flag_AnimateAlly { get; set; }
    [FlatBufferItem(49)] public bool Flag_Dance { get; set; }
    [FlatBufferItem(50)] public bool Flag_Metronome { get; set; }

}
