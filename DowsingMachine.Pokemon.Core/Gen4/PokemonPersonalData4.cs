using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Projects;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Games;

[StructLayout(LayoutKind.Sequential)]
public class Personal4
{
    public byte Basic_hp;
    public byte Basic_pow;
    public byte Basic_def;
    public byte Basic_agi;

    public byte Basic_spepow;
    public byte Basic_spedef;
    [StringReference(@"typename")] public byte Type1;
    [StringReference(@"typename")] public byte Type2;

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

    public byte Sex;
    public byte Egg_birth;
    public byte Friend;
    public byte Grow;

    public byte Egg_group1;
    public byte Egg_group2;
    public byte Speabi1;
    public byte Speabi2;

    public byte Escape;
    [BitField(7)] public byte Color;
    [BitField(1)] public byte Reverse;

    public uint Machine1;
    public uint Machine2;
    public uint Machine3;
    public uint Machine4;
}
