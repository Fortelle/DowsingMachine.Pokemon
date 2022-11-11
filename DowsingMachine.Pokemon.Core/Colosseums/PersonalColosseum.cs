using PBT.DowsingMachine.Data;

namespace PBT.DowsingMachine.Pokemon.Core.Projects.Colosseums;

public class PersonalColosseum
{
    public byte Grow;
    public byte Get_rate;
    public byte Sex;
    public byte Unknown04;

    public byte Unknown05;
    public byte Unknown06;
    public ushort Give_exp;

    public ushort Friend;
    public ushort Unknown1;

    public ushort Unknown2;
    public ushort Unknown3;

    public ushort NationalDexNumber;
    public ushort UnknownDexNumber;

    public ushort Unknown4;
    public ushort Unknown5;

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

    public byte Egg_Group1;
    public byte Egg_Group2;

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
