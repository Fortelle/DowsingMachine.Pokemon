using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Games;

public class PokemonProjectVI : PokemonProject3DS
{
    public static int DexPokemonCount = 721;

    private static readonly Dictionary<string, string[]> LanguageMapsORAS = new()
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

    public PokemonProjectVI(GameTitle title, string baseFolder) : base(title, baseFolder)
    {
        switch (title)
        {
            case GameTitle.X:
            case GameTitle.Y:
                AddReference("PersonalTable",
                    new GarcReader(@"romfs\a\2\1\8"),
                    garc => garc[..^1],
                    MarshalArray<Personal6>
                    );
                AddReference("Wazaoboe",
                    new GarcReader(@"romfs\a\2\1\4"),
                    x => ParseEnumerable(x, ReadLevelupMoves)
                    );
                AddReference("EggMoves",
                    new GarcReader(@"romfs\a\2\1\3"),
                    x => ParseEnumerable(x, PokemonProjectV.ReadEggMoves)
                    );
                AddReference("MachineList",
                    new StreamBinaryReader(@"exefs\code.bin", 0x464796),
                    ReadTMHMList
                    );
                break;
            case GameTitle.OmegaRuby:
            case GameTitle.Sapphire:
                AddReference("PersonalTable",
                    new GarcReader(@"romfs\a\1\9\5"),
                    garc => garc[..^1],
                    MarshalArray<Personal6>
                    );
                AddReference("Wazaoboe",
                    new GarcReader(@"romfs\a\1\9\1"),
                    x => ParseEnumerable(x, ReadLevelupMoves)
                    );
                AddReference("EggMoves",
                    new GarcReader(@"romfs\a\1\9\0"),
                    x => ParseEnumerable(x, PokemonProjectV.ReadEggMoves)
                    );
                AddReference("MachineList",
                    new StreamBinaryReader(@"exefs\code.bin", 0x4A67EE),
                    ReadTMHMList
                    );
                AddReference("TutorList",
                    new StreamBinaryReader(@"exefs\code.bin", 0x4960F8),
                    ReadTutorListORAS
                    );
                AddReference("TutorMoveData",
                    new StreamBinaryReader(@"exefs\code.bin", 0x47AD9C),
                    ReadTutorDataORAS
                    );

                AddReference($"message", new MessageReader(@$"romfs\bin\message\", LanguageMapsORAS));

                break;
        }
    }

    public class HGSSTutorMove
    {
        public int Move;
        public int Cost;
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

    public static HGSSTutorMove[] ReadTutorDataORAS(BinaryReader br)
    {
        var list = new List<HGSSTutorMove>();
        for (var i = 0; i < 60; i++)
        {
            list.Add(new HGSSTutorMove()
            {
                Move = br.ReadInt32(),
                Cost = br.ReadInt32(),
            });
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

    public int[] ReadTMHMList(BinaryReader br)
    {
        return Enumerable.Range(0, Game.Title is GameTitle.X or GameTitle.Y ? 105 : 107).Select(_ => (int)br.ReadInt16()).ToArray();
    }

    [Test]
    public PokemonId[] GetPokemonIds()
    {
        var personals = GetData<Personal6[]>("PersonalTable");
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

    [Dump]
    public IEnumerable<string> DumpLearnset()
    {
        Directory.CreateDirectory(OutputPath);

        var suffix = Game.Title switch
        {
            GameTitle.X or GameTitle.Y => "xy",
            GameTitle.OmegaRuby or GameTitle.AlphaSapphire => "omegarubyalphasapphire",
        };
        var personals = GetData<Personal6[]>("PersonalTable");
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
            var path = Path.Combine(OutputPath, $"{suffix}.levelup.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            var tmlist = GetData<int[]>("MachineList");
            var tmlist2 = Game.Title is GameTitle.X or GameTitle.Y
                ? tmlist[0..92].Concat(tmlist[97..]).Concat(tmlist[92..97]).ToArray()
                : tmlist[0..92].Concat(tmlist[98..106]).Concat(tmlist[92..98]).Concat(tmlist[106..]).ToArray(); ;

            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Machine1, personals[i].Machine2, personals[i].Machine3, personals[i].Machine4);
                var data = PokemonUtils.MatchFlags(tmlist2, tm, (x, j) => j switch
                {
                    < 100 => $"{x}:TM{j + 1:00}",
                    _ => $"{x}:HM{j - 99:00}",
                });
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
                var tm = PokemonUtils.ToBooleans(personals[i].Tutor);
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

        if (Game.Title is GameTitle.OmegaRuby or GameTitle.AlphaSapphire)
        {
            var lt = new LearnsetTable();
            var tutorlist = GetData<int[][]>("TutorList").ToArray();
            /*var tutorlist2 = new[] {
                tutorlist[36..52],
                tutorlist[52..],
                tutorlist[0..17],
                tutorlist[17..36],
            };*/

            for (var i = 0; i < personals.Length; i++)
            {
                var data = new[] {
                    personals[i].Tutor1,
                    personals[i].Tutor2,
                    personals[i].Tutor3,
                    personals[i].Tutor4
                }.SelectMany((x, j) =>
                {
                    var flags = PokemonUtils.ToBooleans(x);
                    var d = PokemonUtils.MatchFlags(tutorlist[j], flags); //, (y, k) => $"{y}:{j}"
                    return d;
                }).ToArray();

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
            var eggs = GetData<int[][]>("EggMoves");

            for (var i = 0; i < eggs.Length; i++)
            {
                lt.Add(dexNumbers[i], eggs[i]);
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
            var path = Path.Combine(OutputPath, $"{suffix}.special.txt");
            lt.Save(path, format);
            yield return path;
        }
    }
}