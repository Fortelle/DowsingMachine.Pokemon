using GFMSG;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core.Gen6;

public class PokemonProjectVI : PokemonProject3DS
{
    public static int DexPokemonCount = 721;

    public PokemonProjectVI() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        var variable = new
        {
            pokemon_egg_moves = @"romfs\a\2\1\3",
            pokemon_levelup_moves = @"romfs\a\2\1\4",
            pokemon_personals = @"romfs\a\2\1\8",
            tmhm_move_list = 0x464796,
            tmhm_move_count = 105,

            tutor_move_data_oras = 0,
            tutor_move_list_oras = 0,
            tutor_move_count = 0,
        };

        switch (Title)
        {
            case GameTitle.X or GameTitle.Y:
                LanguageMaps = new()
                {
                    ["ja-Hrkt"] = new[] { @"romfs\a\0\7\2", @"romfs\a\0\8\0" },
                    ["ja-Jpan"] = new[] { @"romfs\a\0\7\3", @"romfs\a\0\8\1" },
                    ["en-US"] = new[] { @"romfs\a\0\7\4", @"romfs\a\0\8\2" },
                    ["fr"] = new[] { @"romfs\a\0\7\5", @"romfs\a\0\8\3" },
                    ["it"] = new[] { @"romfs\a\0\7\6", @"romfs\a\0\8\4" },
                    ["de"] = new[] { @"romfs\a\0\7\7", @"romfs\a\0\8\5" },
                    ["es"] = new[] { @"romfs\a\0\7\8", @"romfs\a\0\8\6" },
                    ["ko"] = new[] { @"romfs\a\0\7\9", @"romfs\a\0\8\7" },
                };

                break;
            case GameTitle.OmegaRuby or GameTitle.Sapphire:
                variable = variable with
                {
                    pokemon_egg_moves = @"romfs\a\1\9\0",
                    pokemon_levelup_moves = @"romfs\a\1\9\1",
                    pokemon_personals = @"romfs\a\1\9\5",
                    tmhm_move_list = 0x4A67EE,
                    tmhm_move_count = 107,

                    tutor_move_data_oras = 0x47AD9C,
                    tutor_move_list_oras = 0x4960F8,
                    tutor_move_count = 0,
                };

                LanguageMaps = new()
                {
                    ["ja-Hrkt"] = new[] { @"romfs\a\0\7\1", @"romfs\a\0\7\9" },
                    ["ja-Jpan"] = new[] { @"romfs\a\0\7\2", @"romfs\a\0\8\0" },
                    ["en-US"] = new[] { @"romfs\a\0\7\3", @"romfs\a\0\8\1" },
                    ["fr"] = new[] { @"romfs\a\0\7\4", @"romfs\a\0\8\2" },
                    ["it"] = new[] { @"romfs\a\0\7\5", @"romfs\a\0\8\3" },
                    ["de"] = new[] { @"romfs\a\0\7\6", @"romfs\a\0\8\4" },
                    ["es"] = new[] { @"romfs\a\0\7\7", @"romfs\a\0\8\5" },
                    ["ko"] = new[] { @"romfs\a\0\7\8", @"romfs\a\0\8\6" },
                };

                break;
        }

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
                .Then(MarshalArray<Personal6>)
        });
        Resources.Add(new DataResource("tmhm_move_list")
        {
            Reference = new FileRef(@"exefs\code.bin"),
            Reader = new FileReader(variable.tmhm_move_list)
                .Then(br => br.ReadShorts(variable.tmhm_move_count))
        });

        Resources.Add(new DataResource("tutor_move_list_oras")
        {
            Enable = variable.tutor_move_list_oras > 0,
            Reference = new FileRef(@"exefs\code.bin"),
            Reader = new FileReader(variable.tutor_move_list_oras)
                .Then(ReadTutorListORAS)
        });

        Resources.Add(new DataResource("msg")
        {
            Reference = new FileRef(@"{0}"),
            Reader = new GarcReader()
                .Then(ParseEnumerable(x => new Lazy<MsgDataV2>(() => new MsgDataV2(x)))),
            Browsable = false,
        });

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

    private static int[] ReadEggMoves(BinaryReader br)
    {
        var list = new List<int>();
        if (br.BaseStream.Length > 0)
        {
            var count = br.ReadInt16();
            for (var i = 0; i < count; i++)
            {
                list.Add(br.ReadInt16());
            }
        }
        return list.ToArray();
    }

    private static int[][] ReadTutorListORAS(BinaryReader br)
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

    [Test]
    public (PokemonId, Personal6)[] GetKeyedPersonals()
    {
        var personals = GetData<Personal6[]>("pokemon_personals");
        var ids = Enumerable.Repeat(PokemonId.Empty, personals.Length).ToArray();
        for (var i = 0; i <= DexPokemonCount; i++)
        {
            ids[i] = new PokemonId(i, 0);
            if (personals[i].Form_stats_start > 0)
            {
                for (var j = 1; j < personals[i].Form_max; j++)
                {
                    if (ids[personals[i].Form_stats_start + j - 1] != PokemonId.Empty) throw new Exception();
                    ids[personals[i].Form_stats_start + j - 1] = new PokemonId(i, j);
                }
            }
        }
        return ids.Zip(personals, (x, y) => (x, y)).ToArray();
    }

    [Data("learnset/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = GetKeyedPersonals();
        var levelup = GetData<LevelupMove[][]>("pokemon_levelup_moves");
        var tmlist = GetData<short[]>("tmhm_move_list");
        var eggs = GetData<int[][]>("pokemon_egg_moves");

        var collection = new LearnsetTableCollection("{0:000}.{1:00}");

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < levelup.Length; i++)
            {
                var data = levelup[i].Select(x => $"{x.Move}:{x.Level}");
                lt.Add(personals[i].Item1, data.ToArray());
            }
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            var tmlist2 = Game.Title is GameTitle.X or GameTitle.Y
                ? tmlist[0..92].Concat(tmlist[97..]).Concat(tmlist[92..97]).ToArray()
                : tmlist[0..92].Concat(tmlist[98..106]).Concat(tmlist[92..98]).Concat(tmlist[106..]).ToArray();

            foreach (var (id, personal) in personals)
            {
                var flags = new FlagArray(personal.Machine1, personal.Machine2, personal.Machine3, personal.Machine4);
                var data = flags.OfTrue(tmlist2, (m, j) => j switch
                {
                    < 100 => $"{m}:TM{j + 1:00}",
                    _ => $"{m}:HM{j - 99:00}",
                });
                lt.Add(id, data);
            }
            collection.Add("tm", lt);
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
            foreach (var (id, personal) in personals)
            {
                var flags = new FlagArray(personal.Tutor);
                var data = flags.OfTrue(tutorlist);
                lt.Add(id, data);
            }
            collection.Add("tutor_ult", lt);
        }

        if (Game.Title is GameTitle.OmegaRuby or GameTitle.AlphaSapphire)
        {
            var lt = new LearnsetTable();
            var tutorlist = GetData<int[][]>("tutor_move_list_oras");

            foreach (var (id, personal) in personals)
            {
                var data = new[] {
                    personal.Tutor1,
                    personal.Tutor2,
                    personal.Tutor3,
                    personal.Tutor4
                }.SelectMany((x, j) =>
                {
                    var flags = new FlagArray(x);
                    var d = flags.OfTrue(tutorlist[j]);
                    return d;
                }).ToArray();

                lt.Add(id, data);
            }
            collection.Add("tutor", lt);
        }

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < eggs.Length; i++)
            {
                lt.Add(personals[i].Item1, eggs[i]);
            }
            collection.Add("egg", lt);
        }

        {
            var lt = new LearnsetTable();
            lt.Add(new PokemonId(175), 344);
            var rotom_form_moves = new[] { 0, 315, 56, 59, 403, 437, };
            for (var i = 1; i < 6; i++)
            {
                lt.Add(new PokemonId(479, i), rotom_form_moves[i]);
            }
            lt.Add(new PokemonId(646, 0), 549, 184);
            lt.Add(new PokemonId(646, 1), 554, 558);
            lt.Add(new PokemonId(646, 2), 553, 559);
            lt.Add(new PokemonId(647, 0), 548);
            lt.Add(new PokemonId(648, 0), 547);
            lt.Add(new PokemonId(648, 1), 547); // ???
            if (Game.Title is GameTitle.OmegaRuby or GameTitle.AlphaSapphire)
            {
                var pikachu_form_moves = new[] { 0, 309, 556, 577, 604, 560, };
                for (var i = 1; i < 6; i++)
                {
                    lt.Add(new PokemonId(25, i), pikachu_form_moves[i]);
                }
            }
            collection.Add("special", lt);
        }

        return collection;
    }
}