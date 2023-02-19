using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;
using Syroot.BinaryData;

namespace PBT.DowsingMachine.Pokemon.Core.Colosseums;

// 03 2D 1F 00 00 40 00 46 00 07 00 45 03 E2 00 01 05 4E 00 01 00 CB 00 00 00 00 03 E9 00 00 06 62 00 00 00 00 00 00 00 07 00 00 01 89 00 00 00 01 0C 03 41 00 00 00 00 00 00 01 00 00 01 01 01 00 00 00 00 00 01 00 01 00 01 01 00 00 00 00 01 00 00 00 00 01 00 00 00 01 00 00 00 00 00 01 01 01 01 00 00 00 00 00 01 00 00 01 01 01 00 00 01 01 00 01 00 00 01 00 01 00 00 00 00 00 00 00 00 71 00 82 00 DB 00 CC 00 50 01 59 01 40 00 AE 00 2D 00 31 00 31 00 41 00 41 00 2D 00 00 00 00 00 00 00 01 00 00 00 00 04 00 00 10 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 21 04 00 00 2D 07 00 00 49 0A 00 00 16 0F 00 00 4D 0F 00 00 4F 14 00 00 4B 19 00 00 E6 20 00 00 4A 27 00 00 EB 2E 00 00 4C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 04 00 00 01 1C 37 32 00 04 00 01 83 1C 38 32 00
// 03 2D 1F 00 00 8D 00 46 00 0A 00 82 03 E3 00 02 05 50 00 02 00 CC 00 00 00 00 03 EA 00 00 06 62 00 00 00 00 00 00 00 08 00 00 01 8A 00 00 00 02 0C 03 41 00 00 00 00 00 00 01 00 00 01 01 01 00 00 00 00 00 01 00 01 00 01 01 00 00 00 00 01 00 00 00 00 01 00 00 00 01 00 00 00 00 00 01 01 01 01 00 00 00 00 00 01 00 00 01 01 01 00 00 01 01 00 01 00 00 01 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 3E 00 3F 00 50 00 50 00 3C 00 00 00 00 00 00 00 01 00 01 00 00 04 00 00 20 00 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 21 01 00 00 2D 01 00 00 49 04 00 00 2D 07 00 00 49 0A 00 00 16 0F 00 00 4D 0F 00 00 4F 16 00 00 4B 1D 00 00 E6 26 00 00 4A 2F 00 00 EB 38 00 00 4C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 04 00 00 02 1C 39 32 00 04 00 01 84 1C 3A 32 00
// 03 2D 1F 00 00 D0 00 46 00 14 03 E8 03 E4 00 03 05 4D 00 03 00 CD 00 00 00 00 03 EB 00 00 06 62 00 00 00 00 00 00 00 09 00 00 01 8B 00 00 00 03 0C 03 41 00 00 00 00 00 01 01 00 00 01 01 01 00 00 00 01 00 01 00 01 00 01 01 00 00 00 01 01 00 00 00 00 01 00 00 00 01 00 00 00 00 00 01 01 01 01 00 00 00 00 00 01 00 00 01 01 01 00 00 01 01 00 01 00 00 01 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 50 00 52 00 53 00 64 00 64 00 50 00 00 00 00 00 00 00 02 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 21 01 00 00 2D 01 00 00 49 01 00 00 16 04 00 00 2D 07 00 00 49 0A 00 00 16 0F 00 00 4D 0F 00 00 4F 16 00 00 4B 1D 00 00 E6 29 00 00 4A 35 00 00 EB 41 00 00 4C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 04 00 00 03 1C 35 32 00 04 00 01 85 1C 36 32 00
// 03 2D 1F 00 00 41 00 46 00 06 00 55 03 E5 00 04 05 4E 00 04 00 CE 00 00 00 00 03 EC 00 00 06 7D 00 00 00 00 00 00 00 0A 00 00 01 8C 00 00 00 04 0A 0A 42 00 01 01 00 00 00 01 00 00 00 01 01 00 00 00 00 00 01 00 00 00 01 00 01 00 00 00 01 01 00 00 01 01 00 00 01 00 00 01 00 01 00 01 01 01 01 00 00 00 00 01 01 00 00 01 00 01 00 00 01 01 01 01 00 00 01 00 01 00 00 00 00 00 00 00 00 BB 00 F6 00 9D 00 2C 00 C8 00 FB 00 0E 01 5D 00 27 00 34 00 2B 00 3C 00 32 00 41 00 00 00 00 00 00 00 00 00 00 00 01 04 00
public class PersonalXD
{
    public byte Grow;
    public byte Get_rate;
    public byte Sex;
    public byte _unknown04;

    public ushort Give_exp;
    public ushort Friend;

    public ushort Unknown1;
    public ushort Unknown2;

    public ushort Unknown3;
    public ushort NationalDexNumber;


    public ushort Unknown4;
    public ushort UnknownDexNumber;
    public ushort Unknown6;

    public ushort UnknownZero;// not needed

    public uint Unknown7;
    public uint Unknown8;

    public uint Unknown20;
    public uint Unknown24;
    public uint Unknown28;
    public uint Unknown2C;

    public byte Type1;
    public byte Type2;
    public byte Speabi1;
    public byte Speabi2;

    [ArraySize(58)]
    public byte[] Machines;
    [ArraySize(12)]
    public byte[] Tutors;

    public ushort Item1;
    public ushort Item2;

    [ArraySize(8)]
    public ushort[] EggMoves;

    public ushort Basic_hp;
    public ushort Basic_pow;
    public ushort Basic_def;
    public ushort Basic_agi;
    public ushort Basic_spepow;
    public ushort Basic_spedef;

    public ushort Pains_hp;
    public ushort Pains_pow;
    public ushort Pains_def;
    public ushort Pains_agi;
    public ushort Pains_spepow;
    public ushort Pains_spedef;

    [ArraySize(5)]
    public EvolutionXD[] Evolutions;

    [ArraySize(20)]
    public LevelMoveXD[] LevelMoves;

    public byte Color;
    public ushort UnknownDexNumber2;
}



public struct DDPK
{
    public byte _unknown01;
    public byte CatchRate;
    public byte Level;
    public byte _unknown04;

    public ushort _empty05;
    public ushort Index;

    public ushort PurificationGauge;
    public ushort _empty0A;

    public ushort ShadowMove1;
    public ushort ShadowMove2;

    public ushort ShadowMove3;
    public ushort ShadowMove4;

    public byte _unknown14;
    public byte _unknown15;
    public byte _empty16;
    public byte _empty17;
}

public struct DPKM
{
    [AsEnum(typeof(Monsname))]
    public short Number;
    public byte Level;
    public byte Happiness;

    public short Item;
    public byte _unknown06;
    public byte _unknown07;

    [ArraySize(6)]
    public byte[] IVs;
    [ArraySize(6)]
    public byte[] EVs;

    public ushort Move1;
    public ushort Move2;
    public ushort Move3;
    public ushort Move4;

    public byte _unknown1C;
    public byte _unknown1D;
    public byte _unknown1E;
    public byte _unknown1F;
}
