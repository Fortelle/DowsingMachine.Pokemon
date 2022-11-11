using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.Gen1;

[StructLayout(LayoutKind.Sequential)]
public struct Personal1
{
    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    // ‾‾ ‾‾ ‾‾ ‾‾
    public byte OldNumber;
    public byte HitPoint;
    public byte AttackPoint;
    public byte DefenceRatio;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //             ‾‾ ‾‾ ‾‾ ‾‾
    public byte QuickRatio;
    public byte SpecialAbility;
    public byte Type1;
    public byte Type2;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                         ‾‾ ‾‾ ‾‾ ‾‾
    public byte ObtainRatio;
    public byte ExperienceValue;
    public byte MonsterYXSize;
    public byte FRONT_ADS1;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                                     ‾‾ ‾‾ ‾‾ ‾‾
    public byte FRONT_ADS2;
    public byte BACK_ADS1;
    public byte BACK_ADS2;
    public byte Skill1;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                                                 ‾‾ ‾‾ ‾‾ ‾‾
    public byte Skill2;
    public byte Skill3;
    public byte Skill4;
    public byte PatternOfExperience;

    // 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B
    //                                                             ‾‾‾‾‾‾‾‾‾‾‾ ‾‾‾‾‾‾‾‾‾‾‾
    public uint Machine1;
    public uint Machine2;

}
