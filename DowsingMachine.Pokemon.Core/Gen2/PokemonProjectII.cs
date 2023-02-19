using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.Gen1;
using PBT.DowsingMachine.Projects;
using System.ComponentModel;
using System.Text;

namespace PBT.DowsingMachine.Pokemon.Core.Gen2;

public class PokemonProjectII : FileProject, IPokemonProject
{
    [Option]
    [TypeConverter(typeof(EnumSelectConverter))]
    [Select(GameTitle.Gold, GameTitle.Silver, GameTitle.Crystal)]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }

    public PokemonProjectII() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);

        var positions = new
        {
            pokemon_personals = 0u,
            tmhm_move_list = 0u,
            pokemon_evolution_table = 0u,
            pokemon_egg_moves = 0u,
        };

        switch (Title)
        {
            case GameTitle.Gold or GameTitle.Silver:
                positions = positions with
                {
                    pokemon_personals = 0x51AA9,
                    tmhm_move_list = 0x11A00,
                    pokemon_evolution_table = 0x4295F,
                    pokemon_egg_moves = 0x23B07,
                };
                break;
            case GameTitle.Crystal:
                positions = positions with
                {
                    pokemon_personals = 0x514BA,
                    tmhm_move_list = 0x11614,
                    pokemon_evolution_table = 0x42753,
                    pokemon_egg_moves = 0x23B8C,
                };
                break;
        }

        Resources.Add(new DataResource("pokemon_personals")
        {
            Reference = new PosRef(positions.pokemon_personals),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadByteMatrix(0x20, 251))
                .Then(MarshalArray<Personal2>)
        });

        // 0-49 for tm, 50-56 for hm, 57-59 for tutor(crystal only)
        Resources.Add(new DataResource("tmhm_move_list")
        {
            Reference = new PosRef(positions.tmhm_move_list),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadBytes(60))
                .Then(x => x.ToIntegers())
        });

        Resources.Add(new DataResource("pokemon_evolution_table")
        {
            Reference = new PosRef(positions.pokemon_evolution_table),
            Reader = new DataReader<BinaryReader>()
                .Then(ReadPokemonEvolutions)
        });

        Resources.Add(new DataResource("pokemon_egg_moves")
        {
            Reference = new PosRef(positions.pokemon_egg_moves),
            Reader = new DataReader<BinaryReader>()
                .Then(ReadPokemonEggMoves)
        });

    }

    private static int[][] ReadPokemonEggMoves(BinaryReader br)
    {
        var offsets = Enumerable.Range(0, 251).Select(_ => br.ReadInt16()).ToArray();

        var list = new List<int[]>();
        foreach (var offset in offsets)
        {
            br.BaseStream.Seek(offset + 0x01C000, SeekOrigin.Begin);
            var moves = new List<int>();
            while (true)
            {
                var mi = br.ReadByte();
                if (mi == 0xFF) break;
                moves.Add(mi);
            }
            list.Add(moves.ToArray());
        }

        return list.ToArray();
    }

    private static Evolution[] ReadPokemonEvolutions(BinaryReader br)
    {
        var offsets = Enumerable.Range(1, 251).Select(_ => br.ReadInt16()).ToArray();

        var list = new List<Evolution>();
        foreach (var offset in offsets)
        {
            br.BaseStream.Seek(offset + 0x03C000, SeekOrigin.Begin);
            var evo = new Evolution()
            {
                Methods = new(),
                Moves = new(),
            };

            while (true)
            {
                var ev = br.ReadByte();
                if (ev == 1) // level
                {
                    evo.Methods.Add(new Dictionary<string, int>
                    {
                        ["way"] = ev,
                        ["level"] = br.ReadByte(),
                        ["target"] = br.ReadByte(),
                    });
                }
                else if (ev == 2) // item
                {
                    evo.Methods.Add(new Dictionary<string, int>
                    {
                        ["way"] = ev,
                        ["item"] = br.ReadByte(),
                        ["target"] = br.ReadByte(),
                    });
                }
                else if (ev == 3) // trade
                {
                    evo.Methods.Add(new Dictionary<string, int>
                    {
                        ["way"] = ev,
                        ["value"] = br.ReadSByte(),
                        ["target"] = br.ReadByte(),
                    });
                }
                else if (ev == 4) // happinese
                {
                    evo.Methods.Add(new Dictionary<string, int>
                    {
                        ["way"] = ev,
                        ["value"] = br.ReadByte(),
                        ["target"] = br.ReadByte(),
                    });
                }
                else if (ev == 5) // Tyrogue
                {
                    evo.Methods.Add(new Dictionary<string, int>
                    {
                        ["way"] = ev,
                        ["level"] = br.ReadByte(),
                        ["value"] = br.ReadByte(),
                        ["target"] = br.ReadByte(),
                    });
                }
                else
                {
                    break;
                }
            }

            while (true)
            {
                var lv = br.ReadByte();
                if (lv > 0)
                {
                    var mi = br.ReadByte();
                    evo.Moves.Add(new LevelupMove(mi, lv));
                }
                else
                {
                    break;
                }
            }

            list.Add(evo);
        }

        return list.ToArray();
    }


    [Test]
    public (PokemonId, Personal2)[] GetKeyedPersonals()
    {
        return GetData<Personal2[]>("pokemon_personals")
            .Select((p, i) => (new PokemonId(i + 1), p))
            .ToArray();
    }


    [Data(@"learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = GetKeyedPersonals();
        var evo = GetData<Evolution[]>("pokemon_evolution_table");
        var tmhmtrlist = GetData<int[]>("tmhm_move_list");
        var eggs = GetData<int[][]>("pokemon_egg_moves");

        var collection = new LearnsetTableCollection(@"{0:000}");

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < 251; i++)
            {
                var data = evo[i].Moves.Select(x => $"{x.Move}:{x.Level}").ToArray();
                lt.Add(new(i + 1), data);
            }
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var fa = new FlagArray(personal.Machine1, personal.Machine2);
                fa.Flags = fa.Flags.Take(57).ToArray();
                var tmhmlist = tmhmtrlist.Take(57).ToArray();
                var data = fa.OfTrue(tmhmlist, (m, j) => j < 50 ? $"{m}:TM{j + 1:00}" : $"{m}:HM{j - 49:00}");
                lt.Add(id, data);
            }
            collection.Add("tm", lt);
        }

        if (Game.Title is GameTitle.Crystal)
        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var fa = new FlagArray(personal.Machine1, personal.Machine2);
                fa.Flags = fa.Flags.Skip(57).ToArray();
                var trlist = tmhmtrlist.Skip(57).ToArray();
                var data = fa.OfTrue(trlist, (m) => $"{m}").ToArray();
                lt.Add(id, data);
            }
            collection.Add("tutor", lt);
        }

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < 251; i++)
            {
                lt.Add(new(i + 1), eggs[i]);
            }
            collection.Add("egg", lt);
        }

        return collection;
    }

    [Action]
    public string CompareWithRGBY()
    {
        var old = DowsingMachineApp.FindProject<PokemonProjectI>(s => s
            .MaxBy(x => x.Title)
            );
        if (old is null) return "";

        var sb = new StringBuilder();

        {
            var newLearnsets = DumpLearnsets();
            var oldLearnsets = old.DumpLearnsets();
            var diffs = newLearnsets.CompareWith(oldLearnsets, "tm");
            foreach (var diff in diffs)
            {
                sb.AppendLine($"No.{diff.Pokemon.Number:000} {(Monsname)diff.Pokemon.Number}");
                sb.AppendLine(diff.ToText());
            }
        }

        return sb.ToString();
    }

    [Action]
    [Title(GameTitle.Crystal)]
    public string CompareWithGS()
    {
        var old = DowsingMachineApp.FindProject<PokemonProjectII>(s => s
            .Where(p => p.Title is GameTitle.Gold or GameTitle.Silver)
            .FirstOrDefault()
            );
        if (old is null) return "";

        var sb = new StringBuilder();

        {
            var oldPersonals = old.GetKeyedPersonals().Where(x => x.Item1.IsValid).ToDictionary(x => x.Item1, x => x.Item2);
            var newPersonals = GetKeyedPersonals().Where(x => x.Item1.IsValid).ToDictionary(x => x.Item1, x => x.Item2);

            var ch = new DictionaryComparer<PokemonId>()
            {
                KeyToString = (x) => $"No.{x.Number:000} {(Monsname)x.Number}",
                IgnoreProperties = new[]
                {
                    "Machine1",
                    "Machine2",
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
            foreach (var diff in diffs)
            {
                sb.AppendLine($"No.{diff.Pokemon.Number:000} {(Monsname)diff.Pokemon.Number}");
                sb.AppendLine(diff.ToText());
            }
        }

        return sb.ToString();
    }

}
