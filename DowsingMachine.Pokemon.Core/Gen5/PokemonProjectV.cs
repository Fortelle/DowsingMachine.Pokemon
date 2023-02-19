using GFMSG;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core.Gen5;

public class PokemonProjectV : PokemonProjectDS
{
    private record B2W2TutorMove(int Index, int Costs,int Order);
    
    public static int DexPokemonCount = 649;

    public PokemonProjectV() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        Resources.Add(new DataResource("pokemon_personals")
        {
            Reference = new FileRef(@"root\a\0\1\6"),
            Reader = new NarcReader()
                .Then(data => data[..^1])
                .Then(MarshalArray<Personal5>),
        });
        Resources.Add(new DataResource("pokemon_chihou_number")
        {
            Reference = new FileRef(@"root\a\0\1\6"),
            Reader = new NarcReader()
                .Then(data => data[^1])
                .Then(MarshalTo<short>)
        });
        Resources.Add(new DataResource("pokemon_levelup_moves")
        {
            Reference = new FileRef(@"root\a\0\1\8"),
            Reader = new NarcReader()
                .Then(ParseEnumerable(ReadLevelupMoves)),
        });
        Resources.Add(new DataResource("msg")
        {
            Reference = new FileRef(@"root\a\0\0\2"),
            Reader = new NarcReader()
                .Then(ParseEnumerable(x => new MsgDataV2(x))),
        });
        Resources.Add(new DataResource("script")
        {
            Reference = new FileRef(@"root\a\0\0\3"),
            Reader = new NarcReader()
                .Then(ParseEnumerable(x => new MsgDataV2(x))),
        });

        switch (Title)
        {
            case GameTitle.Black or GameTitle.White:
                Resources.Add(new DataResource("pokemon_egg_moves")
                {
                    Reference = new FileRef(@"root\a\1\2\3"),
                    Reader = new NarcReader()
                        .Then(ParseEnumerable(ReadEggMoves)),
                });
                Resources.Add(new DataResource("tmhm_move_list")
                {
                    Reference = new FileRef(@"arm9.bin"),
                    Reader = new FileReader(0x9A948)
                        .Then(ReadTMHMList),
                });
                break;
            case GameTitle.Black2 or GameTitle.White2:
                Resources.Add(new DataResource("pokemon_egg_moves")
                {
                    Reference = new FileRef(@"root\a\1\2\4"),
                    Reader = new NarcReader()
                        .Then(ParseEnumerable(ReadEggMoves)),
                });
                Resources.Add(new DataResource("tmhm_move_list")
                {
                    Reference = new FileRef(@"arm9.bin"),
                    Reader = new FileReader(0x8C8C0)
                        .Then(ReadTMHMList),
                });
                Resources.Add(new DataResource("tutor_move_data")
                {
                    Reference = new FileRef(@"root\ftc\overlay9_5"),
                    Reader = new OverlayReader(0x512DC)
                        .Then(ReadTutorListB2W2),
                });
                break;
        }
    }


    private LevelupMove[] ReadLevelupMoves(BinaryReader br)
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

    private int[] ReadTMHMList(BinaryReader br)
    {
        var list = new List<int>();
        while (true)
        {
            var mi = br.ReadUInt16();
            if (mi == 0) break;
            list.Add(mi);
        }
        return list.ToArray();
    }

    private int[] ReadEggMoves(BinaryReader br)
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

    private static B2W2TutorMove[] ReadTutorListB2W2(BinaryReader br)
    {
        var list = new List<B2W2TutorMove>();
        for (var i = 0; i < 60; i++)
        {
            list.Add(new B2W2TutorMove(
                br.ReadInt32(),
                br.ReadInt32(),
                br.ReadInt32()
                ));
        }
        return list.ToArray();
    }

    [Data]
    public MultilingualCollection DumpMessage()
    {
        var langcodes = Language switch
        {
            "jpn" => new[] { "ja-Hrkt", "ja-Jpan" },
            "eng" => new[] { "en-US" },
            "fra" => new[] { "fr" },
            "ita" => new[] { "it" },
            "ger" => new[] { "de" },
            "spa" => new[] { "es" },
            "kor" => new[] { "ko" },
            _ => new[] { "" }
        };

        var msg = GetData<MsgDataV2[]>("msg");
        var script = GetData<MsgDataV2[]>("script");

        var msgwrappers = msg.Select((x, i) => new MsgWrapper(x, $"msg\\{i}", FileVersion.GenV, langcodes));
        var scriptwrappers = script.Select((x, i) => new MsgWrapper(x, $"script\\{i}", FileVersion.GenV, langcodes));

        var mc = new MultilingualCollection
        {
            Formatter = new GFMSG.Pokemon.BwMsgFormatter(),
        };
        mc.Wrappers.Add(Language ?? "", msgwrappers.Concat(scriptwrappers).ToArray());
        return mc;
    }


    [Test]
    public (PokemonId, Personal5)[] GetKeyedPersonals()
    {
        var personals = GetData<Personal5[]>("pokemon_personals");
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

    [Data("learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = GetKeyedPersonals();
        var levelup = GetData<LevelupMove[][]>("pokemon_levelup_moves");
        var tmlist = GetData<int[]>("tmhm_move_list");
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
            var tmlist2 = tmlist[0..92].Concat(tmlist[98..]).Concat(tmlist[92..98]).ToArray();

            foreach (var (id, personal) in personals)
            {
                var flags = new FlagArray(personal.Machine1, personal.Machine2, personal.Machine3, personal.Machine4);
                var data = flags.OfTrue(tmlist, (m, j) => j switch
                {
                    < 92 => $"{m}:TM{j + 1:00}",
                    < 98 => $"{m}:HM{j - 92 + 1:00}",
                    _ => $"{m}:TM{j - 6 + 1:00}",
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
            };

            foreach (var (id, personal) in personals)
            {
                var flags = new FlagArray(personal.Tutor);
                var data = flags.OfTrue(tutorlist);
                lt.Add(id, data);
            }
            collection.Add("tutor_ult", lt);
        }

        if (Game.Title is GameTitle.Black2 or GameTitle.White2)
        {
            var trlist = GetData<B2W2TutorMove[]>("tutor_move_list");
            var tutorlist2 = trlist.Select(x => x.Index).ToArray();
            var tutorlists2 = new[] {
                tutorlist2[13..28],
                tutorlist2[43..],
                tutorlist2[0..13],
                tutorlist2[28..43],
            };

            var lt = new LearnsetTable();

            foreach (var (id, personal) in personals)
            {
                var data = new[] {
                    personal.Tutor1,
                    personal.Tutor2,
                    personal.Tutor3,
                    personal.Tutor4
                }
                .Select(x => new FlagArray(x))
                .Zip(tutorlists2, (flags, list) => flags.OfTrue(list))
                .SelectMany(x => x)
                .ToArray();
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
            lt.Add(new PokemonId(647, 0), 548);
            lt.Add(new PokemonId(648, 0), 547);
            lt.Add(new PokemonId(648, 1), 547); // ???
            if (Game.Title is GameTitle.Black2 or GameTitle.White2)
            {
                lt.Add(new PokemonId(646, 0), 549, 184);
                lt.Add(new PokemonId(646, 1), 554, 558);
                lt.Add(new PokemonId(646, 2), 553, 559);
            }
            collection.Add("special", lt);
        }

        return collection;
    }
}
