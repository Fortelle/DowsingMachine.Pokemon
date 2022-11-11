using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core;
using PBT.DowsingMachine.Pokemon.Core.Gen7;
using PBT.DowsingMachine.Projects;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Games;

public partial class PokemonProjectVII : PokemonProject3DS
{
    public record TamagoWazaData(ushort AnotherFormHeadDataID, ushort WazaNum, ushort[] Wazano);

    public string SourceCodePath { get; set; }

    public int DexPokemonCount => Game.Title switch
    {
        GameTitle.Sun or GameTitle.Moon => 802,
        GameTitle.UltraSun or GameTitle.UltraMoon => 807,
        GameTitle.LetsGoPikachu or GameTitle.LetsGoEevee => 809,
    };

    private static readonly Dictionary<string, string[]> LanguageMaps = new()
    {
        ["ja-Hrkt"] = new[] { @"romfs\a\0\3\0", @"romfs\a\0\4\0" },
        ["ja-Jpan"] = new[] { @"romfs\a\0\3\1", @"romfs\a\0\4\1" },
        ["en-US"] = new[] { @"romfs\a\0\3\2", @"romfs\a\0\4\2" },
        ["fr"] = new[] { @"romfs\a\0\3\3", @"romfs\a\0\4\3" },
        ["it"] = new[] { @"romfs\a\0\3\4", @"romfs\a\0\4\4" },
        ["de"] = new[] { @"romfs\a\0\3\5", @"romfs\a\0\4\5" },
        ["es"] = new[] { @"romfs\a\0\3\6", @"romfs\a\0\4\6" },
        ["ko"] = new[] { @"romfs\a\0\3\7", @"romfs\a\0\4\7" },
        ["zh-Hans"] = new[] { @"romfs\a\0\3\8", @"romfs\a\0\4\8" },
        ["zh-Hant"] = new[] { @"romfs\a\0\3\9", @"romfs\a\0\4\9" },
    };

    public PokemonProjectVII(GameTitle title, string baseFolder) : base(title, baseFolder)
    {
        ((IPokemonProject)this).Set(title);

        AddReference("PersonalTable",
            new GarcReader(@"romfs\a\0\1\7"),
            garc => garc[..^1],
            MarshalArray<Personal7>
            );
        AddReference("Wazaoboe",
            new GarcReader(@"romfs\a\0\1\3"),
            x => ParseEnumerable(x, ReadLevelupMoves)
            );
        AddReference("EggMoves",
            new GarcReader(@"romfs\a\0\1\2"),
            x => ParseEnumerable(x, ReadEggMoves)
            );

        AddReference("item",
            new GarcReader(@"romfs\a\0\1\9"),
            MarshalArray<Item7>
            );

        switch (title)
        {
            case GameTitle.Sun:
                AddReference("MachineList",
                    new StreamBinaryReader(@"exefs\code.bin", 0x49795A),
                    ReadTMHMList
                    );
                break;
            case GameTitle.Moon:
                break;
            case GameTitle.UltraSun:
                AddReference("MachineList",
                    new StreamBinaryReader(@"exefs\code.bin", 0x4BB98E),
                    ReadTMHMList
                    );
                AddReference("BpWazaOshieTable",
                    new StreamBinaryReader(@"exefs\code.bin", 0x4D9A7C),
                    ReadBpWazaOshieTable
                    );
                AddReference("TutorList2",
                    new StreamBinaryReader(@"exefs\code.bin", 0x4E6860),
                    br => Enumerable.Range(0, 67).Select(x => (int)br.ReadInt16()).ToArray()
                    );
                break;
            case GameTitle.UltraMoon:
                break;
        }

        AddReference($"message", new MessageReader(@$"romfs\bin\message\", LanguageMaps));
    }

    public static TamagoWazaData ReadEggMoves(BinaryReader br)
    {
        const int MAX_EGG_WAZA_NUM = 31;
        var index = br.ReadUInt16();
        var count = br.ReadUInt16();

        var list = new ushort[MAX_EGG_WAZA_NUM];
        for (int i = 0; i < count; i++)
        {
            var move = br.ReadUInt16();
            list[i] = move;
        }

        return new TamagoWazaData(index, count, list);
    }


    public static int[][] ReadTutorListORAS(BinaryReader br)
    {
        var list = new List<List<int>>();
        for (var i = 0; i < 4; i++)
        {
            var list2 = new List<int>();
            while (true)
            {
                var move = br.ReadInt16();
                if (move == 0x026E) break;// maxmovecount
                list2.Add(move);
            }
            list.Add(list2);
        }
        return list.Select(x => x.ToArray()).ToArray();
    }

    public static LevelupMove[] ReadLevelupMoves(BinaryReader br)
    {
        var list = new List<LevelupMove>();
        while (true)
        {
            var mi = br.ReadUInt16();
            var lv = br.ReadUInt16();
            if (mi == 0xFFFF && lv == 0xFFFF) break;
            list.Add(new LevelupMove(mi, lv));
        }
        return list.ToArray();
    }

    public int[] ReadTMHMList(BinaryReader br)
    {
        return Enumerable.Range(0, 100).Select(_ => (int)br.ReadInt16()).ToArray();
    }

    public int[][] ReadBpWazaOshieTable(BinaryReader br)
    {
        var list = new List<int[]>();
        for(var i = 0; i < 4; i++)
        {
            var list2 = new List<int>();
            while (true)
            {
                var move = br.ReadInt16();
                list2.Add(move);
                if (move == 0x02d9) break;
            }
            list.Add(list2.ToArray());
        }
        return list.ToArray();
    }


    [Test]
    public PokemonId[] GetPokemonIds()
    {
        var personals = GetData<Personal7[]>("PersonalTable");
        var ids = Enumerable.Repeat(PokemonId.Empty, personals.Length).ToArray();
        for (var i = 0; i <= DexPokemonCount; i++)
        {
            ids[i] = new PokemonId(i, 0);
            if (personals[i].Form_index > 0)
            {
                for (var j = 1; j < personals[i].Form_max; j++)
                {
                    if (ids[personals[i].Form_index + j - 1] != PokemonId.Empty) throw new Exception();
                    ids[personals[i].Form_index + j - 1] = new PokemonId(i, j);
                }
            }
        }
        return ids.ToArray();
    }

    [Test]
    public PokemonId[] GetPokemonEggIds()
    {
        var personals = GetData<Personal7[]>("PersonalTable");
        var eggs = GetData<TamagoWazaData[]>("EggMoves");
        var ids = Enumerable.Repeat(PokemonId.Empty, eggs.Length).ToArray();
        for (var i = 0; i <= DexPokemonCount; i++)
        {
            ids[i] = new PokemonId(i, 0);
            if (eggs[i].AnotherFormHeadDataID > DexPokemonCount)
            {
                for (var j = 1; j < personals[i].Form_max; j++)
                {
                    if (ids[eggs[i].AnotherFormHeadDataID + j - 1].Form == 1) break;//;throw new Exception();
                    ids[eggs[i].AnotherFormHeadDataID + j - 1] = new PokemonId(i, j);
                }
            }
        }
        var x = Array.FindIndex(ids, id => id == "671.04");
        ids[x+1] = new PokemonId(671, 5);
        return ids;
    }

    [Extraction]
    public string ExtractPersonal()
    {
        var personal = GetData<Personal5[]>("PersonalTable")[..^1];
        var path = Path.Combine(OutputPath, $"personal.json");
        JsonUtil.Serialize(path, personal);
        return path;
    }

    [Extraction]
    public IEnumerable<string> ExtractLearnset()
    {
        Directory.CreateDirectory(OutputPath);

        var suffix = Game.Title switch
        {
            GameTitle.Sun or GameTitle.Moon => "sunmoon",
            GameTitle.UltraSun or GameTitle.UltraMoon => "ultrasunultramoon",
        };

        var personals = GetData<Personal7[]>("PersonalTable");
        var dexNumbers = GetPokemonIds();
        var format = "{0:000}.{1:00}";

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < personals.Length; i++)
            {
                if (personals[i].Zenryoku_waza_item == 0)
                {
                    lt.Add(dexNumbers[i]);
                    //continue;
                }
                else{
                    lt.Add(dexNumbers[i], $"{personals[i].Zenryoku_waza_after}:{personals[i].Zenryoku_waza_before}");
                }
            }
            var path = Path.Combine(OutputPath, $"{suffix}.zmove.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            var moves = GetData<LevelupMove[][]>("Wazaoboe");
            for (var i = 0; i < moves.Length; i++)
            {
                var data = moves[i].Select(x => $"{x.Move}:{x.Level}");
                lt.Add(dexNumbers[i], data.ToArray());
            }
            var path = Path.Combine(OutputPath, $"{suffix}.levelup.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            var tmlist = GetData<int[]>("MachineList");

            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Machine1, personals[i].Machine2, personals[i].Machine3, personals[i].Machine4);
                var data = PokemonUtils.MatchFlags(tmlist, tm, (x, j) => $"{x}:TM{j + 1:00}");
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tm.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputPath, $"{suffix}.tmlist.json");
            JsonUtil.Serialize(path2, tmlist);
            yield return path2;
        }

        {
            var lt = new LearnsetTable();
            var tutorlist = new int[] {
                0x0208,
                0x0207,
                0x0206,
                0x0152,
                0x0133,
                0x0134,
                0x01B2,
                0x026C
            };

            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Waza_oshie_kyukyoku);
                var data = PokemonUtils.MatchFlags(tutorlist, tm);
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tutor_ult.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputPath, $"{suffix}.tutorultlist.json");
            JsonUtil.Serialize(path2, tutorlist);
            yield return path2;
        }

        if (Game.Title is GameTitle.UltraSun or GameTitle.UltraMoon)
        {
            var lt = new LearnsetTable();
            var tutorlist = GetData<int[]>("TutorList2").ToArray();

            for (var i = 0; i < personals.Length; i++)
            {
                var flags = PokemonUtils.ToBooleans(new[] {
                    personals[i].Waza_oshie_momiji1,
                    personals[i].Waza_oshie_momiji2,
                    personals[i].Waza_oshie_momiji3
                });
                var data = PokemonUtils.MatchFlags(tutorlist, flags);

                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tutor.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputPath, $"{suffix}.tutorultlist.json");
            JsonUtil.Serialize(path2, tutorlist);
            yield return path2;
        }

        {
            var lt = new LearnsetTable();
            var eggs = GetData<TamagoWazaData[]>("EggMoves");
            var ids = GetPokemonEggIds();

            for (var i = 0; i < eggs.Length; i++)
            {
                var data = eggs[i].Wazano.TakeWhile(x => x > 0).Select(x => (int)x).ToArray();
                Debug.Assert(data.Length == eggs[i].WazaNum);
                lt.Add(ids[i], data);
            }

            var path = Path.Combine(OutputPath, $"{suffix}.egg.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputPath, $"{suffix}.tamagowaza.json");
            JsonUtil.Serialize(path2, eggs);
            yield return path2;
        }

        {
            var lt = new LearnsetTable();

            void SetAllForm(int pi, int move)
            {
                var max = personals[pi].Form_max;
                for (var i = 0; i < max; i++)
                {
                    lt.Add(new PokemonId(pi, i), move);
                }
            }

            // poke_lib\pml\src\pokepara\pml_EggGenerator.cpp => DecideEggParam_Waza_PITYUU
            lt.Add(new PokemonId(175), 344);

            // poke_lib\pml\src\pokepara\rotom_waza.cdat => rotom_waza_tbl
            var rotom_waza_tbl = new[] { 0, 315, 56, 59, 403, 437, };
            for (var i = 0; i < 6; i++)
            {
                lt.Add(new PokemonId(479, i), rotom_waza_tbl[i]);
            }

            // poke_lib\pml\src\pokepara\pml_PokemonCoreParam.cpp => ChangeKyuremuFormWaza
            var change_waza_table = new[] {
                new[]{ 184, 558, 559 },
                new[]{ 549, 554, 553 },
            };
            for(var i = 0; i < 3; i++)
            {
                lt.Add(new PokemonId(646, i), change_waza_table[0][i], change_waza_table[1][i]);
            }

            // niji_project\prog\Field\FieldStatic\source\Script\ScriptFuncSetInit.cpp => CheckWazaOshieMons
            SetAllForm(384, 620);

            SetAllForm(648, 547);
            SetAllForm(647, 548);

            SetAllForm(25, 344);

            // niji_project\prog\App\Bag\source\BagFrame.cpp => GetUnionWaza
            if (Game.Title is GameTitle.UltraSun or GameTitle.UltraMoon)
            {
                lt.Add(new PokemonId(800, 1), 713);
                lt.Add(new PokemonId(800, 2), 714);
            }

            var path = Path.Combine(OutputPath, $"{suffix}.special.txt");
            lt.Save(path, format);
            yield return path;
        }
    }

}
