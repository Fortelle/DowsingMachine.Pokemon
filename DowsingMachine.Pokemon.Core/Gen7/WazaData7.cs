using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Projects;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Games;

[StructLayout(LayoutKind.Sequential)]
[StringReference(@"message\113")]
public class WazaData7
{
    // 0x00-0x03
    [StringReference(@"message\107")] public byte Type;
    public byte Category;
    public byte DamageType;
    public byte Power;

    // 0x04-0x07
    public byte HitPercent;
    public byte BasePP;
    public sbyte Priority;
    [BitField(4)] public byte HitCountMin;
    [BitField(4)] public byte HitCountMax;

    // 0x08-0x0B
    public ushort SickID;
    public byte SickPercent;
    public byte SickContinuity;

    // 0x0C-0x0F
    public byte SickTurnMin;
    public byte SickTurnMax;
    public byte CriticalRank;
    public byte ShrinkPercent;

    // 0x10-0x13
    public ushort AiSequenceNumber;
    public sbyte DamageRecoverRatio;
    public sbyte HPRecoverRatio;

    // 0x14-0x17
    public byte Target;
    [ArraySize(3)] public byte[] RankEffectTypes;

    // 0x18-0x1F
    [ArraySize(3)] public sbyte[] RankEffectValues;
    [ArraySize(3)] public byte[] RankEffectPercents;
    [StringReference(@"message\113")] public ushort ZenryokuWazaNo;

    // 0x20-0x23
    public byte ZenryokuPower;
    public byte ZenryokuEffect;
    public byte DirtType;
    public byte DirtRate;

    // 0x24
    public FlagValue<uint, WazaFlags7> Flags;
}

public enum WazaFlags7
{
    Touch,
    Tame,
    Tire,
    Mamoru,
    MagicCoat,
    Yokodori,
    Oumugaeshi,
    Punch,
    Sound,
    Flying,
    KooriMelt,
    TripleFar,
    KaifukuHuuji,
    MigawariThru,
    SkyBattleFail,
    EffectToFriend,
    Dance,
};
