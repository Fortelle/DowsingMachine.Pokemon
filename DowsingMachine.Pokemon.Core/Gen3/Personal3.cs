using PBT.DowsingMachine.Data;

namespace PBT.DowsingMachine.Pokemon.Core.Gen3;

public struct Personal3
{
    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    // ‾‾ ‾‾ ‾‾ ‾‾
    public byte Basic_hp;
    public byte Basic_pow;
    public byte Basic_def;
    public byte Basic_agi;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //             ‾‾ ‾‾ ‾‾ ‾‾
    public byte Basic_spepow;
    public byte Basic_spedef;
    public byte Type1;
    public byte Type2;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                         ‾‾ ‾‾ ‾‾‾‾‾
    public byte Get_rate;
    public byte Give_exp;

    [BitField(2)] public ushort Pains_hp;
    [BitField(2)] public ushort Pains_pow;
    [BitField(2)] public ushort Pains_def;
    [BitField(2)] public ushort Pains_agi;
    [BitField(2)] public ushort Pains_spepow;
    [BitField(2)] public ushort Pains_spedef;
    [BitField(4)] public ushort _;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                                     ‾‾‾‾‾ ‾‾‾‾‾
    public ushort Item1;
    public ushort Item2;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                                                 ‾‾ ‾‾ ‾‾ ‾‾
    public byte Sex;
    public byte Egg_birth;
    public byte Friend;
    public byte Grow;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                                                             ‾‾ ‾‾ ‾‾ ‾‾
    public byte Egg_group1;
    public byte Egg_group2;
    public byte Speabi1;
    public byte Speabi2;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                                                                         ‾‾ ‾‾
    public byte Escape;
    [BitField(7)] public byte Color;
    [BitField(1)] public byte Reverse;
}
