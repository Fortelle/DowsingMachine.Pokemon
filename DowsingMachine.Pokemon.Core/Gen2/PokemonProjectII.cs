using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.Gen1;
using PBT.DowsingMachine.Projects;
using System.Text;

namespace PBT.DowsingMachine.Pokemon.Core.Gen2;

public class PokemonProjectII : SingleFileProject, IPokemonProject
{
    public GameInfo Game { get; set; }

    private const int MONS_TBL_SIZE = 0x0020;

    public PokemonProjectII(GameTitle title, string baseFile) : base(title.ToString(), baseFile)
    {
        ((IPokemonProject)this).Set(title);

        switch (title)
        {
            case GameTitle.Gold or GameTitle.Silver:
                AddReference("PersonalTable", new SingleReader(0x51AA9), ReadPokemon);
                AddReference("TMHMList", new SingleReader(0x11A00), ReadTMHMList);
                AddReference("EvolutionTable", new SingleReader(0x4295F), ReadEvolution);
                AddReference("EggMoves", new SingleReader(0x23B07), ReadEggMoves);
                break;
            case GameTitle.Crystal:
                AddReference("PersonalTable", new SingleReader(0x514BA), ReadPokemon);
                AddReference("TMHMList", new SingleReader(0x11614), ReadTMHMList);
                AddReference("EvolutionTable", new SingleReader(0x42753), ReadEvolution);
                AddReference("EggMoves", new SingleReader(0x23B8C), ReadEggMoves);
                break;
        }
    }

    private static Personal2[] ReadPokemon(BinaryReader br)
    {
        return Enumerable.Range(0, 251)
            .Select(_ => br.ReadBytes(MONS_TBL_SIZE))
            .Select(x => MarshalUtil.Deserialize<Personal2>(x))
            .ToArray();
    }

    private static int[][] ReadEggMoves(BinaryReader br)
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
    private static Evolution[] ReadEvolution(BinaryReader br)
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

    private static int[] ReadTMHMList(BinaryReader br)
    {
        // 0-49 for tm, 50-56 for hm, 57-59 for tutor(crystal only)
        return br.ReadBytes(60).Select(x => (int)x).ToArray();
    }

    [Dump]
    public string DumpPersonal()
    {
        var personal = GetData<Personal2[]>("PersonalTable");

        var path = Path.Combine(OutputPath, $"personal.json");
        JsonUtil.Serialize(path, personal);
        return path;
    }

    [Dump]
    public IEnumerable<string> DumpLearnset()
    {
        var personals = GetData<Personal2[]>("PersonalTable");
        var suffix = Game.Title.ToString().ToLower();
        var tmlist = GetData<int[]>("TMHMList");

        {
            var sb = new StringBuilder();
            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Machine1, personals[i].Machine2);
                var data = tmlist.Select((m, j) => new { m, j }).Take(57).Where(x => tm[x.j]).Select(x => x.j < 50 ? $"{x.m}:TM{x.j + 1:00}" : $"{x.m}:HM{x.j - 49:00}");
                var line = string.Join(",", data);
                sb.AppendLine($"{personals[i].No:000}\t{line}");
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tm.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

        if (Game.Title == GameTitle.Crystal)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Machine1, personals[i].Machine2);
                var data = tmlist.Select((m, j) => new { m, j }).Skip(57).Where(x => tm[x.j]).Select(x => $"{x.m}");
                var line = string.Join(",", data);
                sb.AppendLine($"{personals[i].No:000}\t{line}");
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tutor.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

        {
            var sb = new StringBuilder();
            var evo = GetData<Evolution[]>("EvolutionTable");
            for (var i = 0; i < 251; i++)
            {
                var data = evo[i].Moves.Select(x => $"{x.Move}:{x.Level}");
                var line = string.Join(",", data);
                sb.AppendLine($"{i + 1:000}\t{line}");
            }
            var path = Path.Combine(OutputPath, $"{suffix}.levelup.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

        {
            var sb = new StringBuilder();
            var eggs = GetData<int[][]>("EggMoves");
            for (var i = 0; i < 251; i++)
            {
                var line = string.Join(",", eggs[i]);
                sb.AppendLine($"{i + 1:000}\t{line}");
            }
            var path = Path.Combine(OutputPath, $"{suffix}.egg.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }
    }
}
