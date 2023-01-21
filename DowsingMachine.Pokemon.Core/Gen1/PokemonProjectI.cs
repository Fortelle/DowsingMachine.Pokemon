using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Utilities;

namespace PBT.DowsingMachine.Pokemon.Core.Gen1;

public class PokemonProjectI : FileProject, IPokemonProject
{
    [Option]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }

    public const int InternalPokemonCount = 0xBE;

    public bool IsRGB => Title is GameTitle.Red
        or GameTitle.Green
        or GameTitle.Blue
        or GameTitle.RedVC
        or GameTitle.BlueVC
        or GameTitle.GreenVC;

    public bool IsVC => Title is GameTitle.RedVC
        or GameTitle.BlueVC
        or GameTitle.GreenVC
        or GameTitle.YellowVC;

    public PokemonProjectI() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);

        switch (Title)
        {
            case GameTitle.Red:
            case GameTitle.Green:
            case GameTitle.Blue:
                AddReference("PersonalMew", new SingleReader(0x4200), ReadPokemon(1));
                AddReference("PersonalTable", new SingleReader(0x38000), ReadPokemon(150));
                AddReference("TMHMList", new SingleReader(0x12276), ReadTMHMList);
                AddReference("EvolutionTable", new SingleReader(0x3B427), ReadEvolution);
                AddReference("DexOrder", new SingleReader(0x4279A), br => br.ReadBytes(InternalPokemonCount));
                break;
            case GameTitle.Yellow:
                AddReference("PersonalTable", new SingleReader(0x383DE), ReadPokemon(151));
                AddReference("TMHMList", new SingleReader(0x1286C), ReadTMHMList);
                AddReference("DexOrder", new SingleReader(0x4282D), br => br.ReadBytes(InternalPokemonCount));
                AddReference("EvolutionTable", new SingleReader(0x3B59C), ReadEvolution);
                break;
            case GameTitle.RedVC:
            case GameTitle.BlueVC:
                AddReference("PersonalMew", new SingleReader(0x425B), ReadPokemon(1));
                AddReference("PersonalTable", new SingleReader(0x383DE), ReadPokemon(150));
                AddReference("TMHMList", new SingleReader(0x13773), ReadTMHMList);
                AddReference("EvolutionTable", new SingleReader(0x3B05C), ReadEvolution);
                AddReference("DexOrder", new SingleReader(0x41024), br => br.ReadBytes(InternalPokemonCount));
                break;
        }
    }

    public static Func<BinaryReader, Personal1[]> ReadPokemon(int count)
    {
        return br => Enumerable.Range(0, count)
            .Select(_ => br.ReadBytes(28))
            .Select(x => MarshalUtil.Deserialize<Personal1>(x))
            .ToArray();
    }

    public Personal1[] GetPokemon()
    {
        if (IsRGB)
        {
            return GetData<Personal1[]>("PersonalTable")
                .Concat(GetData<Personal1[]>("PersonalMew"))
                .ToArray();
        }
        else
        {
            return GetData<Personal1[]>("PersonalTable").ToArray();
        }
    }

    public Evolution[] ReadEvolution(BinaryReader br)
    {
        var offsets = Enumerable.Range(0, InternalPokemonCount).Select(_ => br.ReadInt16()).ToArray();

        var list = new List<Evolution>();
        foreach (var offset in offsets)
        {
            br.BaseStream.Seek(offset + 0x034000, SeekOrigin.Begin);

            var evo = new Evolution()
            {
                Methods = new(),
                Moves = new(),
            };

            while (true)
            {
                var ev = br.ReadByte();
                if (ev == 0) break;
                var data = new Dictionary<string, int>
                {
                    { "condition", ev }
                };
                if (ev == 2)
                {
                    data.Add("item", br.ReadByte());
                }
                data.Add("level", br.ReadByte());
                data.Add("mons", br.ReadByte());
                evo.Methods.Add(data);
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

    public static int[] ReadTMHMList(BinaryReader br)
    {
        // 0-49 for tm, 50-54 for hm
        return br.ReadBytes(55).Select(x => (int)x).ToArray();
    }

    [Dump]
    public string DumpData()
    {
        var personal = GetPokemon();

        var path = Path.Combine(OutputFolder, $"personal.json");
        JsonUtil.Serialize(path, personal);
        return path;
    }

    [Dump]
    public IEnumerable<string> DumpLearnset()
    {
        var personals = GetPokemon();
        var dexNumbers = GetData<byte[]>("DexOrder");
        var suffix = Game.Title switch
        {
            GameTitle.Red => "redgreen",
            GameTitle.Green => "redgreen",
            GameTitle.Blue => "redgreen",
            GameTitle.Yellow => "yellow",
            GameTitle.RedVC => "redgreenvc",
            GameTitle.BlueVC => "redgreenvc",
            GameTitle.YellowVC => "yellowvc",
        };
        var format = "{0:000}";

        // basic
        {
            var lt = new LearnsetTable();
            for (var i = 0; i < personals.Length; i++)
            {
                var data = new int[] { personals[i].Skill1, personals[i].Skill2, personals[i].Skill3, personals[i].Skill4 };
                lt.Add(new PokemonId(personals[i].OldNumber), data);
            }
            var path = Path.Combine(OutputFolder, "txt", $"{suffix}.basic.txt");
            lt.Save(path, format);
            yield return path;

            path = Path.Combine(OutputFolder, "json", $"{suffix}.basic.json");
            lt.SaveJson(path, false, false);
            yield return path;
        }

        // levelup
        {
            var lt = new LearnsetTable();
            var evo = GetData<Evolution[]>("EvolutionTable");
            for (var i = 0; i < InternalPokemonCount; i++)
            {
                var data = evo[i].Moves.Select(x => $"{x.Move}:{x.Level}").ToArray();
                lt.Add(new PokemonId(dexNumbers[i]), data);
            }
            var path = Path.Combine(OutputFolder, "txt", $"{suffix}.levelup.txt");
            lt.Save(path, format);
            yield return path;

            path = Path.Combine(OutputFolder, "json", $"{suffix}.levelup.json");
            lt.SaveJson(path, false, true);
            yield return path;

            path = Path.Combine(OutputFolder, $"{suffix}.levelup.json");
            JsonUtil.Serialize(path, evo.Select(x => x.Moves));
            yield return path;
        }

        // tmhm
        {
            var lt = new LearnsetTable();
            var tmlist = GetData<int[]>("TMHMList");
            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Machine1, personals[i].Machine2)
                    .Take(tmlist.Length).ToArray()
                    ;
                var data = PokemonUtils.MatchFlags(tmlist, tm, (m, j) => j < 50 ? $"{m}:TM{j + 1:00}" : $"{m}:HM{j - 49:00}");
                lt.Add(new PokemonId(personals[i].OldNumber), data);
            }
            var path = Path.Combine(OutputFolder, "txt", $"{suffix}.tm.txt");
            lt.Save(path, format);
            yield return path;

            path = Path.Combine(OutputFolder, "json", $"{suffix}.tm.json");
            lt.SaveJson(path, false, true);
            yield return path;

            path = Path.Combine(OutputFolder, $"{suffix}.tm.json");
            JsonUtil.Serialize(path, tmlist);
            yield return path;
        }

    }
}
