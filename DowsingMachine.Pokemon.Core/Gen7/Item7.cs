using PBT.DowsingMachine.Data;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.Gen7;

[StructLayout(LayoutKind.Sequential)]
public class Item7
{
    public ushort Price;
    public byte Equip;
    public byte Attack;

    public byte TuibamuEffect;
    public byte NageEffect;
    public byte NageAttack;
    public byte ShizenAttack;

    [BitField(5)] public ushort ShizenType;
    [BitField(1)] public ushort Important;
    [BitField(1)] public ushort ConvenientButton;
    [BitField(4)] public ushort FieldPocket;
    [BitField(5)] public ushort BattlePocket;
    public byte FieldFunction;
    public byte BattleFunction;

    public WorkType WorkType;
    public byte ItemType;
    [BitField(4)] public byte Spend; // battle item, 0=spend, 1=nospend
    [BitField(4)] public byte UseSpend; // use item, 0=spend, 1=nospend
    public byte Sort;

    public ItemWork7 Work;
}

[StructLayout(LayoutKind.Sequential)]
public class ItemWork7
{
    [BitField(1)] public byte SleepRecovery;
    [BitField(1)] public byte PoisonRecovery;
    [BitField(1)] public byte BurnRecovery;
    [BitField(1)] public byte IceRecovery;
    [BitField(1)] public byte ParalyzeRecovery;
    [BitField(1)] public byte PanicRecovery;
    [BitField(1)] public byte MeromeroRecovery;
    [BitField(1)] public byte AbilityGuard;

    [BitField(1)] public byte DeathRecovery;
    [BitField(1)] public byte AllDeathRecovery;
    [BitField(1)] public byte LevelUp;
    [BitField(1)] public byte Evolution;
    [BitField(4)] public byte AtkUp;

    [BitField(4)] public byte DefUp;
    [BitField(4)] public byte SpaUp;

    [BitField(4)] public byte SpdUp;
    [BitField(4)] public byte AgiUp;

    [BitField(4)] public byte HitUp;
    [BitField(2)] public byte CriticalUp;
    [BitField(1)] public byte PpUp;
    [BitField(1)] public byte PpUp3;

    [BitField(1)] public byte PpRecovery;
    [BitField(1)] public byte AllPpRecovery;
    [BitField(1)] public byte HpRecovery;
    [BitField(1)] public byte HpExp;
    [BitField(1)] public byte PowExp;
    [BitField(1)] public byte DefExp;
    [BitField(1)] public byte AgiExp;
    [BitField(1)] public byte SpaExp;

    [BitField(1)] public byte SpdExp;
    [BitField(1)] public byte Exp_limit;
    [BitField(1)] public byte Friend_exp1;
    [BitField(1)] public byte Friend_exp2;
    [BitField(1)] public byte Friend_exp3;
    [BitField(3)] public byte _;

    public sbyte HpExpValue;
    public sbyte PowExpValue;
    public sbyte DefExpValue;
    public sbyte AgiExpValue;

    public sbyte SpaExpValue;
    public sbyte SpdExpValue;
    public byte HpRecoveryValue;  // 255=100%, 254=50%, 253=25%
    public byte PpRecoveryValue;  // 127=100%

    public sbyte Friend1Value;
    public sbyte Friend2Value;
    public sbyte Friend3Value;
    public byte __;
}

public enum WorkType : byte
{
    Normal = 0,
    PokeUse,
    Ball,
};
