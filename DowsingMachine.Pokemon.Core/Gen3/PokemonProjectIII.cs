using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;
using System.Text;

namespace PBT.DowsingMachine.Pokemon.Core.Gen3;

public class PokemonProjectIII : SingleFileProject, IPokemonProject
{
    public GameInfo Game { get; set; }

    public int InternalPokemonCount = 412;

    public PokemonProjectIII(GameTitle title, string baseFile) : base(title.ToString(), baseFile)
    {
        ((IPokemonProject)this).Set(title);

        switch (title)
        {
            case GameTitle.Ruby:
            case GameTitle.Sapphire:
                AddReference("PersonalTable", new SingleReader(0x1D09CC), br => Enumerable.Range(0, InternalPokemonCount).Select(_ => br.ReadBytes(28)).Select(MarshalUtil.Deserialize<Personal3>).ToArray());
                AddReference("DexNumber", new SingleReader(0x1CE2CA), br => Enumerable.Range(0, InternalPokemonCount - 1).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("HoennDexNumber", new SingleReader(0x1CDF94), br => Enumerable.Range(0, InternalPokemonCount - 1).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("TMHMList", new SingleReader(0x35017C), br => Enumerable.Range(0, 60).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("LevelupMoves", new SingleReader(0x1D997C), ReadLevelupMoves);
                AddReference("MachineMoves", new SingleReader(0x1CEEA4), ReadMachineMoveFlags);
                AddReference("EggMoves", new SingleReader(0x1DAF78), ReadEggMoves);
                break;
            case GameTitle.FireRed:
                AddReference("PersonalTable", new SingleReader(0x21118C), br => Enumerable.Range(0, InternalPokemonCount).Select(_ => br.ReadBytes(28)).Select(MarshalUtil.Deserialize<Personal3>).ToArray());
                AddReference("DexNumber", new SingleReader(0x20E9F6), br => Enumerable.Range(0, InternalPokemonCount - 1).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("HoennDexNumber", new SingleReader(0x20E6C0), br => Enumerable.Range(0, InternalPokemonCount - 1).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("TMHMList", new SingleReader(0x419D34), br => Enumerable.Range(0, 58).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("TutorList", new SingleReader(0x4192F0), br => Enumerable.Range(0, 16).Select(_ => (int)br.ReadInt16()).ToArray());

                AddReference("LevelupMoves", new SingleReader(0x21A1BC), ReadLevelupMoves);
                AddReference("MachineMoves", new SingleReader(0x20F5D0), ReadMachineMoveFlags);
                AddReference("TutorMoves", new SingleReader(0x41930E), ReadTutorMoveFlags);
                AddReference("EggMoves", new SingleReader(0x21B918), ReadEggMoves);
                break;
            case GameTitle.LeafGreen:
                AddReference("PersonalTable", new SingleReader(0x21118C), br => Enumerable.Range(0, InternalPokemonCount).Select(_ => br.ReadBytes(28)).Select(MarshalUtil.Deserialize<Personal3>).ToArray());
                AddReference("DexNumber", new SingleReader(0x20E9D2), br => Enumerable.Range(0, InternalPokemonCount - 1).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("HoennDexNumber", new SingleReader(0x20E69C), br => Enumerable.Range(0, InternalPokemonCount - 1).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("TMHMList", new SingleReader(0x419CBC), br => Enumerable.Range(0, 58).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("TutorList", new SingleReader(0x419278), br => Enumerable.Range(0, 16).Select(_ => (int)br.ReadInt16()).ToArray());

                AddReference("LevelupMoves", new SingleReader(0x21A19C), ReadLevelupMoves);
                AddReference("MachineMoves", new SingleReader(0x20F5AC), ReadMachineMoveFlags);
                AddReference("TutorMoves", new SingleReader(0x419296), ReadTutorMoveFlags);
                AddReference("EggMoves", new SingleReader(0x21B8F8), ReadEggMoves);
                break;
            case GameTitle.Emerald:
                AddReference("PersonalTable", new SingleReader(0x2F0D54), br => Enumerable.Range(0, InternalPokemonCount).Select(_ => br.ReadBytes(28)).Select(MarshalUtil.Deserialize<Personal3>).ToArray());
                AddReference("DexNumber", new SingleReader(0x2EE60A), br => Enumerable.Range(0, InternalPokemonCount - 1).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("HoennDexNumber", new SingleReader(0x2EE2D4), br => Enumerable.Range(0, InternalPokemonCount - 1).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("TMHMList", new SingleReader(0x5E144C), br => Enumerable.Range(0, 58).Select(_ => (int)br.ReadInt16()).ToArray());
                AddReference("TutorList", new SingleReader(0x5E08C4), br => Enumerable.Range(0, 32).Select(_ => (int)br.ReadInt16()).ToArray());

                AddReference("LevelupMoves", new SingleReader(0x2F9D04), ReadLevelupMoves);
                AddReference("MachineMoves", new SingleReader(0x2EF220), ReadMachineMoveFlags);
                AddReference("TutorMoves", new SingleReader(0x5E0900), ReadTutorMoveFlags);
                AddReference("EggMoves", new SingleReader(0x2FB764), ReadEggMoves);
                break;
        }
    }

    public static Dictionary<int, int[]> ReadEggMoves(BinaryReader br)
    {
        var dict = new Dictionary<int, List<int>>();
        List<int> list = null;
        while (true)
        {
            var value = br.ReadUInt16();
            if (value == 0xFFFF)
            {
                break;
            }
            else if (value > 20000)
            {
                list = new List<int>();
                dict.Add(value - 20000, list);
            }
            else
            {
                list.Add(value);
            }
        }
        return dict.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }

    public LevelupMove[][] ReadLevelupMoves(BinaryReader br)
    {
        var offsets = Enumerable.Range(0, InternalPokemonCount).Select(_ => br.ReadInt32()).ToArray();

        var list = new List<LevelupMove[]>();
        foreach (var offset in offsets)
        {
            br.BaseStream.Seek(offset - 0x08000000, SeekOrigin.Begin);
            var ms = new List<LevelupMove>();
            while (true)
            {
                var value = br.ReadUInt16();
                if (value == 0xFFFF) break;
                var lv = value >> 9;
                var mi = value & 0b111111111;
                ms.Add(new LevelupMove(mi, lv));
            }
            list.Add(ms.ToArray());
        }

        return list.ToArray();
    }

    public bool[][] ReadMachineMoveFlags(BinaryReader br)
    {
        var data = Enumerable.Range(0, InternalPokemonCount)
            .Select(_ => br.ReadBytes(8))
            .Select(x => PokemonUtils.ToBooleans(x))
            .ToArray();
        return data;
    }

    public bool[][] ReadTutorMoveFlags(BinaryReader br)
    {
        var data = Enumerable.Range(0, InternalPokemonCount)
            .Select(_ => br.ReadBytes(Game.Title == GameTitle.Emerald ? 4 : 2))
            .Select(x => PokemonUtils.ToBooleans(x))
            .ToArray();
        return data;
    }

    public int[] ReadTMHMList(BinaryReader br)
    {
        // 0-49 for tm, 50-56 for hm, 57-59 for tutor(crystal only)
        return br.ReadBytes(60).Select(x => (int)x).ToArray();
    }

    [Dump]
    public string DumpPersonal()
    {
        var personal = GetData<Personal3[]>("PersonalTable");

        var path = Path.Combine(OutputPath, $"personal.json");
        JsonUtil.Serialize(path, personal, new JsonOptions() { NamePolicy = JsonNamePolicy.Lower });
        return path;
    }

    [Dump]
    public IEnumerable<string> DumpLearnset()
    {
        var suffix = Game.Title switch
        {
            GameTitle.Ruby or GameTitle.Sapphire => "rubysapphire",
            GameTitle.FireRed => "firered",
            GameTitle.LeafGreen => "leafgreen",
            GameTitle.Emerald => "emerald",
        };

        var dexNumbers = GetData<int[]>("DexNumber")
            .Prepend(0)
            .Select(x => x > 386 ? 0 : x)
            .Select(x => x != 386 ? $"{x:000}.00" : Game.Title switch
            {
                GameTitle.Ruby or GameTitle.Sapphire => $"386.00",
                GameTitle.FireRed => $"386.01",
                GameTitle.LeafGreen => $"386.02",
                GameTitle.Emerald => $"386.03",
            })
            .ToArray();

        {
            var sb = new StringBuilder();
            var moves = GetData<LevelupMove[][]>("LevelupMoves");
            for (var i = 0; i < InternalPokemonCount; i++)
            {
                var data = moves[i].Select(x => $"{x.Move}:{x.Level}");
                var line = string.Join(",", data);
                sb.AppendLine($"{dexNumbers[i]}\t{line}");
            }
            var path = Path.Combine(OutputPath, $"{suffix}.levelup.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

        {
            var sb = new StringBuilder();
            var flags = GetData<bool[][]>("MachineMoves");
            var tmlist = GetData<int[]>("TMHMList");

            for (var i = 0; i < InternalPokemonCount; i++)
            {
                var data = tmlist.Select((m, j) => new { m, j }).Where(x => flags[i][x.j]).Select(x => x.j < 50 ? $"{x.m}:TM{x.j + 1:00}" : $"{x.m}:HM{x.j - 49:00}");
                var line = string.Join(",", data);
                sb.AppendLine($"{dexNumbers[i]}\t{line}");
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tm.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

        if (Game.Title == GameTitle.FireRed || Game.Title == GameTitle.LeafGreen || Game.Title == GameTitle.Emerald)
        {
            var sb = new StringBuilder();
            var flags = GetData<bool[][]>("TutorMoves");
            var tmlist = GetData<int[]>("TutorList");

            for (var i = 0; i < InternalPokemonCount; i++)
            {
                var data = tmlist.Select((m, j) => new { m, j }).Where(x => flags[i][x.j]).Select(x => $"{x.m}");
                var line = string.Join(",", data);
                sb.AppendLine($"{dexNumbers[i]}\t{line}");
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tutor.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

        if (Game.Title == GameTitle.FireRed || Game.Title == GameTitle.LeafGreen) // hardcode
        {
            var sb = new StringBuilder();
            sb.AppendLine($"003.00\t338");
            sb.AppendLine($"006.00\t307");
            sb.AppendLine($"009.00\t308");
            var path = Path.Combine(OutputPath, $"{suffix}.tutor_ult.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

        {
            var sb = new StringBuilder();
            var eggs = GetData<Dictionary<int, int[]>>("EggMoves");
            foreach (var (index, moves) in eggs)
            {
                var line = string.Join(",", moves);
                sb.AppendLine($"{dexNumbers[index]}\t{line}");
            }
            var path = Path.Combine(OutputPath, $"{suffix}.egg.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

        if (Game.Title == GameTitle.Emerald) // hardcode
        {
            var sb = new StringBuilder();
            sb.AppendLine($"175.00\t344");
            var path = Path.Combine(OutputPath, $"{suffix}.special.txt");
            File.WriteAllText(path, sb.ToString());
            yield return path;
        }

    }
}
