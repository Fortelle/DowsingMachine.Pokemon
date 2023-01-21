﻿using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Projects;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.Gen7;

[StructLayout(LayoutKind.Sequential)]
[StringReference(@"message\55")]
public class Personal7
{
    public byte Basic_hp;
    public byte Basic_atk;
    public byte Basic_def;
    public byte Basic_agi;

    public byte Basic_spatk;
    public byte Basic_spdef;
    [StringReference(@"message\107")] public byte Type1;
    [StringReference(@"message\107")] public byte Type2;

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

    [StringReference(@"message\36")] public ushort Item1;
    [StringReference(@"message\36")] public ushort Item2;

    [StringReference(@"message\36")] public ushort Item3;
    public byte Sex;
    public byte Egg_Birth;

    public byte Initial_Friendship;
    public byte Grow;
    public byte Egg_Group1;
    public byte Egg_Group2;

    [StringReference(@"message\96")] public byte Tokusei1;
    [StringReference(@"message\96")] public byte Tokusei2;
    [StringReference(@"message\96")] public byte Tokusei3;
    public byte Sos_rate;

    public ushort Form_index;
    public ushort Form_gra_index;

    public byte Form_max;
    [BitField(6)] public byte Color;
    [BitField(2)] public byte _padding_1;
    public ushort Give_Exp;

    public ushort Height;
    public ushort Weight;

    public uint Machine1;
    public uint Machine2;
    public uint Machine3;
    public uint Machine4;

    public uint Waza_oshie_kyukyoku;

    public uint Waza_oshie_momiji1; // _padding_2 in sm
    public uint Waza_oshie_momiji2; // _padding_3 in sm
    public uint Waza_oshie_momiji3; // _padding_4 in sm
    public uint _padding_5;

    [StringReference(@"message\36")] public ushort Zenryoku_waza_item;
    [StringReference(@"message\113")] public ushort Zenryoku_waza_before;

    [StringReference(@"message\113")] public ushort Zenryoku_waza_after;
    public byte Region;
    public byte _padding_6;
}
