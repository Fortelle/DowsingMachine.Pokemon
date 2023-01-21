using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Utilities;

namespace PBT.DowsingMachine.Pokemon.Core.Gen5;

public class PokemonProjectV : PokemonProjectDS
{
    public static int DexPokemonCount = 649;

    public PokemonProjectV() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        AddReference("PersonalTable",
            new NarcReader(@"root\a\0\1\6"),
            data => data[..^1],
            MarshalArray<Personal5>
            );
        AddReference("Wazaoboe",
            new NarcReader(@"root\a\0\1\8"),
            x => ParseEnumerable(x, ReadLevelupMoves)
            );
        /*
        AddReference("common",
            new NarcReader(@"root\a\0\0\2"),
            x => x.Select(y => new PokemonText(y)).ToArray()
            );
        AddReference("script",
            new NarcReader(@"root\a\0\0\3"),
            x => x.Select(y => new PokemonText(y)).ToArray()
            );
        */
        AddReference("msg",
            new NarcReader(@"root\a\0\0\2"),
            ReadMessage
            );
        AddReference("script",
            new NarcReader(@"root\a\0\0\3"),
            ReadMessage
            );

        switch (Title)
        {
            case GameTitle.Black:
            case GameTitle.White:
                AddReference("MachineList",
                    new StreamBinaryReader(@"arm9.bin", 0x9A948),
                    ReadTMHMList
                    );
                AddReference("EggMoves",
                    new NarcReader(@"root\a\1\2\3"),
                    x => ParseEnumerable(x, ReadEggMoves)
                    );
                break;
            case GameTitle.Black2:
            case GameTitle.White2:
                AddReference("MachineList",
                    new StreamBinaryReader(@"arm9.bin", 0x8C8C0),
                    ReadTMHMList
                    );
                AddReference("EggMoves",
                    new NarcReader(@"root\a\1\2\4"),
                    x => ParseEnumerable(x, ReadEggMoves)
                    ); ;
                AddReference("TutorMoveData",
                    new OverlayReader(@"root\ftc\overlay9_5", 0x512DC),
                    ReadTutorListB2W2
                    );
                break;
        }

    }

    public new MultilingualCollection ReadMessage(byte[][] narcData)
    {
        var mc = new MultilingualCollection
        {
            Formatter = new BwMsgFormatter(),
        };

        var wrappers = narcData.Select((data, i) =>
        {
            var msg = new MsgDataV2(data);
            var mw = new MsgWrapper(msg, i.ToString(), FileVersion.GenV, new[] { LanguageCode });
            return mw;
        }).ToArray();

        mc.Wrappers.Add(LanguageCode, wrappers);
        return mc;
    }


    public class B2W2TutorMove
    {
        public int Index;
        public int Costs;
        public int Order;
    }

    public static B2W2TutorMove[] ReadTutorListB2W2(BinaryReader br)
    {
        var list = new List<B2W2TutorMove>();
        for (var i = 0; i < 60; i++)
        {
            list.Add(new B2W2TutorMove()
            {
                Index = br.ReadInt32(),
                Costs = br.ReadInt32(),
                Order = br.ReadInt32(),
            });
        }
        return list.ToArray();
    }

    public static int[] ReadEggMoves(BinaryReader br)
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

    public static int[] ReadTMHMList(BinaryReader br)
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

    [Test]
    public PokemonId[] GetPokemonIds()
    {
        var personals = GetData<Personal5[]>("PersonalTable");
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
        return ids.ToArray();
    }

    [Test]
    public bool[][] GetPokemonTm()
    {
        var tm = GetData<Personal5[]>("PersonalTable").Select(x => PokemonUtils.ToBooleans(x.Machine1, x.Machine2, x.Machine3, x.Machine4));
        return tm.ToArray();
    }

    [Extraction]
    public string ExtractPersonal()
    {
        var personal = GetData<Personal5[]>("PersonalTable");
        var path = Path.Combine(OutputFolder, $"personal.json");
        JsonUtil.Serialize(path, personal);
        return path;
    }

    [Extraction]
    public IEnumerable<string> ExtractLearnset()
    {
        Directory.CreateDirectory(OutputFolder);

        var suffix = Game.Title switch
        {
            GameTitle.Black or GameTitle.White => "blackwhite",
            GameTitle.Black2 or GameTitle.White2 => "black2white2",
        };
        var personals = GetData<Personal5[]>("PersonalTable");
        var dexNumbers = GetPokemonIds();
        var format = "{0:000}.{1:00}";

        {
            var lt = new LearnsetTable();
            var moves = GetData<LevelupMove[][]>("Wazaoboe");
            for (var i = 0; i < moves.Length; i++)
            {
                var data = moves[i].Select(x => $"{x.Move}:{x.Level}");
                lt.Add(dexNumbers[i], data.ToArray());
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.levelup.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            var tmlist = GetData<int[]>("MachineList");
            var tmlist2 = tmlist[0..92].Concat(tmlist[98..]).Concat(tmlist[92..98]).ToArray();

            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Machine1, personals[i].Machine2, personals[i].Machine3, personals[i].Machine4);
                var data = PokemonUtils.MatchFlags(tmlist2, tm, (x, j) => j switch
                {
                    < 95 => $"{x}:TM{j + 1:00}",
                    _ => $"{x}:HM{j - 94:00}",
                });
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.tm.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputFolder, $"{suffix}.tmlist.json");
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
            };

            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Tutor);
                var data = PokemonUtils.MatchFlags(tutorlist, tm);
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.tutor_ult.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputFolder, $"{suffix}.tutorultlist.json");
            JsonUtil.Serialize(path2, tutorlist);
            yield return path2;
        }

        if (Game.Title is GameTitle.Black2 or GameTitle.White2)
        {
            var lt = new LearnsetTable();
            var secBegin = new int[] { 13, 43, 0, 28 };
            var tutorlist = GetData<B2W2TutorMove[]>("TutorMoveData").Select(x => x.Index).ToArray();

            for (var i = 0; i < personals.Length; i++)
            {
                var tutorArray = new[] { personals[i].Tutor1, personals[i].Tutor2, personals[i].Tutor3, personals[i].Tutor4 };
                var data = tutorArray.SelectMany((x, j) =>
                {
                    var flags = PokemonUtils.ToBooleans(x);
                    var tutorlist2 = tutorlist[Range.StartAt(secBegin[j])];
                    var d = PokemonUtils.MatchFlags(tutorlist2, flags); //, (y, k) => $"{y}:{j}"
                    return d;
                }).ToArray();

                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.tutor.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputFolder, $"{suffix}.tutorultlist.json");
            JsonUtil.Serialize(path2, tutorlist);
            yield return path2;
        }

        {
            var lt = new LearnsetTable();
            var eggs = GetData<int[][]>("EggMoves");

            for (var i = 0; i < eggs.Length; i++)
            {
                lt.Add(dexNumbers[i], eggs[i]);
            }

            var path = Path.Combine(OutputFolder, $"{suffix}.egg.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputFolder, $"{suffix}.tamagowaza.json");
            JsonUtil.Serialize(path2, eggs);
            yield return path2;
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
            var path = Path.Combine(OutputFolder, $"{suffix}.special.txt");
            lt.Save(path, format);
            yield return path;
        }
    }
}
