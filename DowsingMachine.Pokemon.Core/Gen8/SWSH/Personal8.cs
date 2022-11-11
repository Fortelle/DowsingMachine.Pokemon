using PBT.DowsingMachine.Data;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

[StructLayout(LayoutKind.Sequential)]
public class Personal8
{
    // 0x00
    public byte Basic_hp;
    public byte Basic_atk;
    public byte Basic_aef;
    public byte Basic_agi;

    // 0x04
    public byte Basic_spatk;
    public byte Basic_spdef;
    public byte Type1;
    public byte Type2;

    // 0x08
    public byte Get_rate;
    public byte Rank;
    [BitField(2)] public ushort Pains_hp;
    [BitField(2)] public ushort Pains_atk;
    [BitField(2)] public ushort Pains_def;
    [BitField(2)] public ushort Pains_agi;
    [BitField(2)] public ushort Pains_spatk;
    [BitField(2)] public ushort Pains_spdef;
    [BitField(1)] public ushort No_jump;
    [BitField(3)] public ushort Pains_yobi;

    // 0x0C
    public ushort Item1;
    public ushort Item2;

    // 0x10
    public ushort Item3;
    public byte Sex;
    public byte Egg_Birth;

    // 0x14
    public byte Friendship;
    public byte Grow;
    public byte Egg_Group1;
    public byte Egg_Group2;

    // 0x18
    public short Tokusei1;
    public short Tokusei2;

    // 0x1C
    public short Tokusei3;
    public short Form_Index;

    // 0x20
    public byte Form_Max;
    [BitField(6)] public byte Color;
    [BitField(1)] public byte Enable;
    [BitField(1)] public byte IsFlippable;
    public ushort Give_Exp;

    // 0x24
    public ushort Height;
    public ushort Weight;

    // 0x28
    public uint Machine1;
    public uint Machine2;
    public uint Machine3;
    public uint Machine4;

    // 0x38
    public uint Hiden_Machine;

    // 0x3C
    public uint Record1;
    public uint Record2;
    public uint Record3;
    public uint Record4;

    // 0x4C
    public ushort Go_monsno; // ?
    public ushort Unknown1;

    // 0x50
    public ushort Zenryoku_waza_item;
    public ushort Zenryoku_waza_before;

    // 0x54
    public ushort Zenryoku_waza_after;
    public ushort Egg_monsno;

    // 0x58
    public ushort Egg_formno;
    [BitField(2)] public byte IsRegionalForm;
    [BitField(2)] public byte DisabledDynamax;

    // 0x5C
    public ushort Chihou_zukan_no;
    public ushort Unknown5;

    [ArraySize(0x48)] public byte[] Unknown6;

    // 0xA8
    public uint Tutor;

    // 0xAC
    public ushort Armor_zukan_no;
    public ushort Crown_zukan_no;

}
