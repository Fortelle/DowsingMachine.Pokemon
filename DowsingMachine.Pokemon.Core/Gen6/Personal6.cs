using PBT.DowsingMachine.Data;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.Gen6;

[StructLayout(LayoutKind.Sequential)]
public struct Personal6
{
    public byte Basic_hp;
    public byte Basic_pow;
    public byte Basic_def;
    public byte Basic_agi;

    public byte Basic_spepow;
    public byte Basic_spedef;
    public byte Type1;
    public byte Type2;

    public byte Get_rate;
    public byte Give_exp;
    [BitField(2)] public ushort Pains_hp;
    [BitField(2)] public ushort Pains_pow;
    [BitField(2)] public ushort Pains_def;
    [BitField(2)] public ushort Pains_agi;
    [BitField(2)] public ushort Pains_spepow;
    [BitField(2)] public ushort Pains_spedef;
    [BitField(4)] public ushort Pains_yobi;

    public ushort Item1;
    public ushort Item2;

    public ushort Item3;
    public byte Sex;
    public byte Egg_Birth;

    public byte Friend;
    public byte Grow;
    public byte Egg_group1;
    public byte Egg_group2;

    public byte Speabi1;
    public byte Speabi2;
    public byte Speabi3;
    public byte Escape;

    public ushort Form_stats_start;
    public ushort Form_sprites_start;
    public byte Form_max;
    [BitField(6)] public byte Color;
    [BitField(1)] public byte NoColor;
    [BitField(1)] public byte Reverse;

    public ushort Give_Exp;
    public ushort Height;
    public ushort Weight;

    public uint Machine1;
    public uint Machine2;
    public uint Machine3;
    public uint Machine4;

    public uint Tutor;

    public ushort _0x3C3D;
    public ushort _0x3E3F;

    public uint Tutor1;
    public uint Tutor2;
    public uint Tutor3;
    public uint Tutor4;

}
