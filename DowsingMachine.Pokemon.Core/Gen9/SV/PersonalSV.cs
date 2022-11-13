using FlatSharp.Attributes;
using PBT.DowsingMachine.Projects;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PBT.DowsingMachine.Pokemon.Core.Gen9;

[FlatBufferTable]
public class PersonalSV
{
    [StringReference(@"monsname")] public int Number => NumForm.Number;

    [FlatBufferItem(00)] public PersonalSVNumForm NumForm { get; set; }
    [FlatBufferItem(01)] public bool HasForm { get; set; }
    [FlatBufferItem(02)] public bool Enable { get; set; }

    [FlatBufferItem(03)][StringReference(@"typename")] public byte Type1 { get; set; }
    [FlatBufferItem(04)][StringReference(@"typename")] public byte Type2 { get; set; }
    [FlatBufferItem(05)][StringReference(@"tokusei")] public ushort Tokusei1 { get; set; }

    [FlatBufferItem(06)][StringReference(@"tokusei")] public ushort Tokusei2 { get; set; }
    [FlatBufferItem(07)][StringReference(@"tokusei")] public ushort Tokusei3 { get; set; }

    [FlatBufferItem(08)] public byte Grow { get; set; }
    [FlatBufferItem(09)] public byte Get_Rate { get; set; }
    [FlatBufferItem(10)] public PersonalSVSex Sex { get; set; }
    [FlatBufferItem(11)] public byte EggGroup1 { get; set; }

    [FlatBufferItem(12)] public byte EggGroup2 { get; set; }
    [FlatBufferItem(13)] public PersonalSVEgg Egg { get; set; }
    [FlatBufferItem(14)] public byte Egg_Birth { get; set; }
    [FlatBufferItem(15)] public byte Frindship { get; set; }

    [FlatBufferItem(16)] public Byte2 Item16 { get; set; }
    [FlatBufferItem(17)] public byte Rank { get; set; }
    [FlatBufferItem(18)] public PersonalSVItem Item { get; set; }
    [FlatBufferItem(19)] public PersonalSVStats Pains { get; set; }

    [FlatBufferItem(20)] public PersonalSVStats Basic { get; set; }
    [FlatBufferItem(21)] public ushort[] EvolutionLevels { get; set; }

    [FlatBufferItem(22)] public ushort[] Waza_machine { get; set; }
    [FlatBufferItem(23)] public ushort[] Waza_egg { get; set; }
    [FlatBufferItem(24)] public ushort[] Waza_tutor { get; set; }
    [FlatBufferItem(25)] public PersonalSVWazaoboe[] Waza_level { get; set; }
}


[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct PersonalSVStats
{
    [FieldOffset(0)] public byte Hp;
    [FieldOffset(1)] public byte Atk;
    [FieldOffset(2)] public byte Def;
    [FieldOffset(3)] public byte Spatk;
    [FieldOffset(4)] public byte Spdef;
    [FieldOffset(5)] public byte Agi;

    public override string ToString()
    {
        return $"{Hp}/{Atk}/{Def}/{Spatk}/{Spdef}/{Agi}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct PersonalSVGet_Rate
{
    [FieldOffset(0)] public byte Get_Rate1;
    [FieldOffset(1)] public byte Get_Rate2;

    public override string ToString()
    {
        return $"{Get_Rate1}/{Get_Rate2}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct PersonalSVWazaoboe
{
    [FieldOffset(0)] public ushort Waza;

    public PersonalSVWazaoboe(ushort waza) : this()
    {
        Waza = waza;
    }

    [FieldOffset(2)] public sbyte Level;

    public override string ToString()
    {
        return $"{Waza}:{Level}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct PersonalSVEgg
{
    [FieldOffset(0)] public ushort Number;
    [FieldOffset(2)] public ushort Form;

    public override string ToString()
    {
        return $"{Number}:{Form}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct PersonalSVNumForm
{
    [FieldOffset(0)] public ushort Number;
    [FieldOffset(2)] public byte Form;

    public override string ToString()
    {
        return $"{Number}.{Form}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct PersonalSVItem
{
    [FieldOffset(0)] public bool HasItem;
    [FieldOffset(2)] public ushort Item;

    public override string ToString()
    {
        return $"{HasItem},{Item}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct UShort2
{
    [FieldOffset(0)] public ushort Value1;
    [FieldOffset(2)] public ushort Value2;

    public override string ToString()
    {
        return $"{Value1},{Value2}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct Byte2
{
    [FieldOffset(0)] public byte Value1;
    [FieldOffset(1)] public byte Value2;

    public override string ToString()
    {
        return $"{Value1},{Value2}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct PersonalSVSex
{
    [FieldOffset(0)] public byte Type; // 0=random, 1=male, 2=female, 3=genderless
    [FieldOffset(1)] public byte Ratio;

    public override string ToString()
    {
        return $"{Type},{Ratio}";
    }
}


[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct Byte8
{
    [FieldOffset(0)] public byte Value1;
    [FieldOffset(1)] public byte Value2;
    [FieldOffset(2)] public byte Value3;
    [FieldOffset(3)] public byte Value4;
    [FieldOffset(4)] public byte Value5;
    [FieldOffset(5)] public byte Value6;
    [FieldOffset(6)] public byte Value7;
    [FieldOffset(7)] public byte Value8;

    public override string ToString()
    {
        return $"{Value1},{Value2},{Value3},{Value4},{Value5},{Value6},{Value7},{Value8}";
    }
}

[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct Byte16
{
    [FieldOffset(0)] public byte Value1;
    [FieldOffset(1)] public byte Value2;
    [FieldOffset(2)] public byte Value3;
    [FieldOffset(3)] public byte Value4;
    [FieldOffset(4)] public byte Value5;
    [FieldOffset(5)] public byte Value6;
    [FieldOffset(6)] public byte Value7;
    [FieldOffset(7)] public byte Value8;

    [FieldOffset(8)] public byte Value9;
    [FieldOffset(9)] public byte Value10;
    [FieldOffset(10)] public byte Value11;
    [FieldOffset(11)] public byte Value12;
    [FieldOffset(12)] public byte Value13;
    [FieldOffset(13)] public byte Value14;
    [FieldOffset(14)] public byte Value15;
    [FieldOffset(15)] public byte Value16;

    public override string ToString()
    {
        return $"{Value1},{Value2},{Value3},{Value4},{Value5},{Value6},{Value7},{Value8},{Value9},{Value10},{Value11},{Value12},{Value13},{Value14},{Value15},{Value16}";
    }
}


[FlatBufferStruct]
[StructLayout(LayoutKind.Explicit)]
public struct Byte12
{
    [FieldOffset(0)] public byte Value1;
    [FieldOffset(1)] public byte Value2;
    [FieldOffset(2)] public byte Value3;
    [FieldOffset(3)] public byte Value4;
    [FieldOffset(4)] public byte Value5;
    [FieldOffset(5)] public byte Value6;
    [FieldOffset(6)] public byte Value7;
    [FieldOffset(7)] public byte Value8;

    [FieldOffset(8)] public byte Value9;
    [FieldOffset(9)] public byte Value10;

    public override string ToString()
    {
        return $"{Value1},{Value2},{Value3},{Value4},{Value5},{Value6},{Value7},{Value8},{Value9},{Value10}";
    }
}
