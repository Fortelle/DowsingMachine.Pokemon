using GFMSG;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;
using System.Diagnostics;
using System.Text;

namespace PBT.DowsingMachine.Pokemon.Core.Gen7;

public partial class PokemonProjectVII : PokemonProject3DS
{
    private record EggMoveEntry(ushort FormNo, ushort Count, ushort[] Moves);

    public int DexPokemonCount => Game?.Title switch
    {
        GameTitle.Sun or GameTitle.Moon => 802,
        GameTitle.UltraSun or GameTitle.UltraMoon => 807,
        GameTitle.LetsGoPikachu or GameTitle.LetsGoEevee => 809,
        _ => 0,
    };

    public PokemonProjectVII() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        var variable = new
        {
            move_data = @"romfs\a\0\1\1",
            pokemon_egg_moves = @"romfs\a\0\1\2",
            pokemon_levelup_moves = @"romfs\a\0\1\3",
            pokemon_personals = @"romfs\a\0\1\7",
            item_data = @"romfs\a\0\1\9",
            tmhm_move_list = 0x49795A,
            tmhm_move_count = 100,

            tutor_move_data_usum = 0,
            tutor_move_list_usum = 0,
            tutor_move_count = 0,
        };

        switch (Title)
        {
            case GameTitle.Sun:

                MessageMaps = new()
                {
                    ["iteminfo"] = @"message\35",
                    ["itemname"] = @"message\36",
                    ["monsname"] = @"message\55",
                    ["place_name"] = @"message\67",
                    ["seikaku"] = @"message\87",
                    ["tokusei"] = @"message\96",
                    ["tokuseiinfo"] = @"message\97",
                    ["trmsg"] = @"message\104",
                    ["trname"] = @"message\105",
                    ["trtype"] = @"message\106",
                    ["typename"] = @"message\107",
                    ["wazainfo"] = @"message\112",
                    ["wazaname"] = @"message\113",
                    ["zkn_form"] = @"message\114",
                    ["zkn_height"] = @"message\115",
                    ["zkn_type"] = @"message\116",
                    ["zkn_weight"] = @"message\117",
                    ["zukan_comment_A"] = @"message\119",
                    ["zukan_comment_B"] = @"message\120",
                    ["zukan_hyouka"] = @"message\121",
                };
                break;
            case GameTitle.Moon:
                break;
            case GameTitle.UltraSun:
                variable = variable with
                {
                    tmhm_move_list = 0x4BB98E,
                    tutor_move_data_usum = 0x4D9A7C,
                    tutor_move_list_usum = 0x4E6860,
                    tutor_move_count = 67,
                };
                MessageMaps = new()
                {
                    ["iteminfo"] = @"message\39",
                    ["itemname"] = @"message\40",
                    ["monsname"] = @"message\60",
                    ["place_name"] = @"message\73",
                    ["seikaku"] = @"message\92",
                    ["tokusei"] = @"message\101",
                    ["tokuseiinfo"] = @"message\102",
                    ["trmsg"] = @"message\109",
                    ["trname"] = @"message\110",
                    ["trtype"] = @"message\111",
                    ["typename"] = @"message\112",
                    ["wazainfo"] = @"message\117",
                    ["wazaname"] = @"message\118",
                    ["zkn_form"] = @"message\119",
                    ["zkn_height"] = @"message\120",
                    ["zkn_type"] = @"message\121",
                    ["zkn_weight"] = @"message\122",
                    ["zukan_comment_A"] = @"message\124",
                    ["zukan_comment_B"] = @"message\125",
                    ["zukan_hyouka"] = @"message\126",
                };
                break;
            case GameTitle.UltraMoon:
                break;


        }

        Resources.Add(new DataResource("move_data")
        {
            Reference = new FileRef(variable.move_data),
            Reader = new GarcReader()
                .Then(garc => new BinData(garc.First()))
                .Then(bin => bin.Values.Select(MarshalUtil.Deserialize<WazaData7>).ToArray()),
        });
        Resources.Add(new DataResource("pokemon_egg_moves")
        {
            Reference = new FileRef(variable.pokemon_egg_moves),
            Reader = new GarcReader()
                .Then(ParseEnumerable(ReadEggMoves))
        });
        Resources.Add(new DataResource("pokemon_levelup_moves")
        {
            Reference = new FileRef(variable.pokemon_levelup_moves),
            Reader = new GarcReader()
                .Then(ParseEnumerable(ReadLevelupMoves))
        });
        Resources.Add(new DataResource("pokemon_personals")
        {
            Reference = new FileRef(variable.pokemon_personals),
            Reader = new GarcReader()
                .Then(garc => garc.SkipLast(1))
                .Then(MarshalArray<Personal7>)
        });
        Resources.Add(new DataResource("item_data")
        {
            Reference = new FileRef(variable.item_data),
            Reader = new GarcReader()
                .Then(MarshalArray<Item7>)
        });
        Resources.Add(new DataResource("tmhm_move_list")
        {
            Reference = new FileRef(@"exefs\code.bin"),
            Reader = new FileReader(variable.tmhm_move_list)
                .Then(br => br.ReadShorts(variable.tmhm_move_count))
        });

        Resources.Add(new DataResource("tutor_move_data_usum")
        {
            Enable = variable.tutor_move_data_usum > 0,
            Reference = new FileRef(@"exefs\code.bin"),
            Reader = new FileReader(variable.tutor_move_data_usum)
                .Then(ReadTutorListBP)
        });
        Resources.Add(new DataResource("tutor_move_list_usum")
        {
            Enable = variable.tutor_move_list_usum > 0,
            Reference = new FileRef(@"exefs\code.bin"),
            Reader = new FileReader(variable.tutor_move_list_usum)
                .Then(br => br.ReadShorts(variable.tutor_move_count))
        });

        Resources.Add(new DataResource("msg")
        {
            Reference = new FileRef(@"{0}"),
            Reader = new GarcReader()
                .Then(ParseEnumerable(x => new Lazy<MsgDataV2>(() => new MsgDataV2(x)))),
            Browsable = false,
        });

        LanguageMaps = new()
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
    }

    private static LevelupMove[] ReadLevelupMoves(BinaryReader br)
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

    private static EggMoveEntry ReadEggMoves(BinaryReader br)
    {
        var index = br.ReadUInt16();
        var count = br.ReadUInt16();

        var list = new ushort[31];
        for (int i = 0; i < count; i++)
        {
            var move = br.ReadUInt16();
            list[i] = move;
        }

        return new EggMoveEntry(index, count, list);
    }

    private static int[][] ReadTutorListBP(BinaryReader br)
    {
        var list = new List<int[]>();
        for (var i = 0; i < 4; i++)
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
    public PokemonId[] GetZukanIndexes()
    {
        var personals = GetData<Personal7[]>("pokemon_personals");
        var lst = new List<PokemonId>();
        for (var i = 0; i <= DexPokemonCount; i++)
        {
            lst.Add(new PokemonId(i));
        }
        lst.Add(PokemonId.Empty);
        for (var i = 0; i <= DexPokemonCount; i++)
        {
            if (personals[i].Form_max > 1)
            {
                for (var j = 1; j < personals[i].Form_max; j++)
                {
                    lst.Add(new PokemonId(i, j));
                }
            }
        }
        return lst.ToArray();
    }

    private string GetPokemonName(PokemonId id)
    {
        var zukanids = GetData(GetZukanIndexes);
        var zukanindex = Array.IndexOf(zukanids, id);
        var monsname = GetString("monsname", id.Number);
        var formname = GetString("zkn_form", zukanindex);
        if (formname != "") monsname += "(" + formname + ")";
        return monsname;
    }

    [Test]
    public (PokemonId, Personal7)[] GetKeyedPersonals()
    {
        var personals = GetData<Personal7[]>("pokemon_personals");
        var ids = Enumerable.Repeat(PokemonId.Empty, personals.Length).ToArray();
        for (var i = 0; i <= DexPokemonCount; i++)
        {
            ids[i] = new PokemonId(i, 0);
            if (personals[i].Form_index > 0)
            {
                for (var j = 1; j < personals[i].Form_max; j++)
                {
                    Debug.Assert(ids[personals[i].Form_index + j - 1] == PokemonId.Empty);
                    ids[personals[i].Form_index + j - 1] = new PokemonId(i, j);
                }
            }
        }
        return ids.Zip(personals, (x, y) => (x, y)).ToArray();
    }

    [Test]
    public (PokemonId, int[])[] GetKeyedEggMoves()
    {
        var personals = GetData<Personal7[]>("pokemon_personals");
        var eggs = GetData<EggMoveEntry[]>("pokemon_egg_moves");
        var ids = Enumerable.Repeat(PokemonId.Empty, eggs.Length).ToArray();
        for (var i = 0; i <= DexPokemonCount; i++)
        {
            ids[i] = new PokemonId(i, 0);
            if (eggs[i].FormNo > DexPokemonCount)
            {
                for (var j = 1; j < personals[i].Form_max; j++)
                {
                    if (ids[eggs[i].FormNo + j - 1].Form == 1) break;//;throw new Exception();
                    ids[eggs[i].FormNo + j - 1] = new PokemonId(i, j);
                }
            }
        }
        //var x = Array.FindIndex(ids, id => id == "671.04");
        //ids[x + 1] = new PokemonId(671, 5);
        return ids.Zip(eggs, (x, y) => (x, y.Moves.Where(x => x > 0).Select(x => (int)x).ToArray())).ToArray();
    }


    [Data("learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = GetKeyedPersonals();
        var eggs = GetKeyedEggMoves();
        var levelups = GetData<LevelupMove[][]>("pokemon_levelup_moves");
        var tmlist = GetData<short[]>("tmhm_move_list");

        var collection = new LearnsetTableCollection("{0:000}.{1:00}");

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < levelups.Length; i++)
            {
                var data = levelups[i].Select(x => $"{x.Move}:{x.Level}");
                lt.Add(personals[i].Item1, data.ToArray());
            }
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                if (personal.Zenryoku_waza_item == 0)
                {
                    lt.Add(id);
                }
                else
                {
                    lt.Add(id, $"{personal.Zenryoku_waza_after}:{personal.Zenryoku_waza_before}");
                }
            }
            collection.Add("zmove", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var flags = new FlagArray(personal.Machine1, personal.Machine2, personal.Machine3, personal.Machine4);
                var data = flags.OfTrue(tmlist, (m, j) => $"{m}:TM{j + 1:00}");
                lt.Add(id, data);
            }
            collection.Add("tm", lt);
        }

        {
            var lt = new LearnsetTable();
            var tutorultlist = new int[] {
                0x0208,
                0x0207,
                0x0206,
                0x0152,
                0x0133,
                0x0134,
                0x01B2,
                0x026C
            };

            foreach (var (id, personal) in personals)
            {
                var flags = new FlagArray(personal.Waza_oshie_kyukyoku);
                var data = flags.OfTrue(tutorultlist);
                lt.Add(id, data);
            }
            collection.Add("tutor_ult", lt);
        }

        if (Game.Title is GameTitle.UltraSun or GameTitle.UltraMoon)
        {
            var tutorlist = GetData<short[]>("tutor_move_list_usum");
            var lt = new LearnsetTable();

            foreach (var (id, personal) in personals)
            {
                var flags = new FlagArray(personal.Waza_oshie_momiji1, personal.Waza_oshie_momiji2, personal.Waza_oshie_momiji3);
                var data = flags.OfTrue(tutorlist);
                lt.Add(id, data);
            }
            collection.Add("tutor", lt);
        }
        
        {
            var lt = new LearnsetTable();

            foreach (var (id, moves) in eggs)
            {
                lt.Add(id, moves);
            }
            collection.Add("egg", lt);
        }

        {
            var lt = new LearnsetTable();

            void SetAllForm(int pi, int move)
            {
                var max = personals[pi].Item2.Form_max;
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
            for (var i = 0; i < 3; i++)
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

            collection.Add("special", lt);
        }

        return collection;
    }

    [Action]
    [Title(GameTitle.UltraSun, GameTitle.UltraMoon)]
    public string CompareWithSM()
    {
        var old = DowsingMachineApp.FindProject<PokemonProjectVII>(s => s
            .Where(p => p.Title is GameTitle.Sun or GameTitle.Moon)
            .FirstOrDefault()
            );
        if (old is null) return "";

        var sb = new StringBuilder();

        {
            var oldPersonals = old.GetKeyedPersonals().Where(x => x.Item1.IsValid).ToDictionary(x => x.Item1, x => x.Item2);
            var newPersonals = GetKeyedPersonals().Where(x => x.Item1.IsValid).ToDictionary(x => x.Item1, x => x.Item2);

            var ch = new DictionaryComparer<PokemonId>()
            {
                KeyToString = (x) => $"No.{x} {GetPokemonName(x)}",
                IgnoreProperties = new[]
                {
                    "Form_index",
                    "Waza_oshie_momiji1",
                    "Waza_oshie_momiji2",
                    "Waza_oshie_momiji3",
                },
            };

            sb.AppendLine("Pokemon");
            sb.AppendLine("----------");
            sb.AppendLine(ch.Compare(oldPersonals, newPersonals));
        }

        {
            var newLearnsets = DumpLearnsets();
            var oldLearnsets = old.DumpLearnsets();
            var diffs = newLearnsets.CompareWith(oldLearnsets);
            var wazaname = GetStrings("wazaname");
            sb.AppendLine("Learnsets");
            sb.AppendLine("----------");
            foreach (var diff in diffs)
            {
                sb.AppendLine($"No.{diff.Pokemon} {GetPokemonName(diff.Pokemon)}");
                sb.AppendLine(diff.ToText(wazaname));
            }
        }

        return sb.ToString();
    }

}
