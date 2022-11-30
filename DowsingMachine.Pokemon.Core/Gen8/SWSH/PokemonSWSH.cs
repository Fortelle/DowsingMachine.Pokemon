using GFMSG;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using PBT.DowsingMachine.Projects;
using System.Text;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public class PokemonProjectSWSH : PokemonProjectNS
{
    public int GetPokemonCount(bool egg = false) => Version switch
    {
        { Major: 1, Minor: 0 or 1 } => 890,
        { Major: 1, Minor: 2 } when egg => 898,
        { Major: 1, Minor: 2 } => 893,
        { Major: 1, Minor: 3 } => 898,
        _ => 0,
    };

    public static readonly int[] TMList =
    {
        005, 025, 006, 007, 008, 009, 019, 042, 063, 416,
        345, 076, 669, 083, 086, 091, 103, 113, 115, 219,
        120, 156, 157, 168, 173, 182, 184, 196, 202, 204,
        211, 213, 201, 240, 241, 258, 250, 251, 261, 263,
        129, 270, 279, 280, 286, 291, 311, 313, 317, 328,
        331, 333, 340, 341, 350, 362, 369, 371, 372, 374,
        384, 385, 683, 409, 419, 421, 422, 423, 424, 427,
        433, 472, 478, 440, 474, 490, 496, 506, 512, 514,
        521, 523, 527, 534, 541, 555, 566, 577, 580, 581,
        604, 678, 595, 598, 206, 403, 684, 693, 707, 784,
    };

    public static readonly int[] TRList =
    {
        014, 034, 053, 056, 057, 058, 059, 067, 085, 087,
        089, 094, 097, 116, 118, 126, 127, 133, 141, 161,
        164, 179, 188, 191, 200, 473, 203, 214, 224, 226,
        227, 231, 242, 247, 248, 253, 257, 269, 271, 276,
        285, 299, 304, 315, 322, 330, 334, 337, 339, 347,
        348, 349, 360, 370, 390, 394, 396, 398, 399, 402,
        404, 405, 406, 408, 411, 412, 413, 414, 417, 428,
        430, 437, 438, 441, 442, 444, 446, 447, 482, 484,
        486, 492, 500, 502, 503, 526, 528, 529, 535, 542,
        583, 599, 605, 663, 667, 675, 676, 706, 710, 776,
    };

    public static readonly int[] TutorList =
    {
        520, 519, 518,
        338, 307, 308,
        434,
        796,
    };

    public static readonly int[] TutorList2 =
    {
        805, 807, 812, 804, 803, 813, 811, 810, 815, 814,
        797, 806, 800, 809, 799, 808, 798, 802,
    };

    public static readonly Dictionary<string, string[]> LanguageMaps = new()
    {
        ["JPN"] = new[] { "ja-Hrkt" },
        ["JPN_KANJI"] = new[] { "ja-Jpan" },
        ["English"] = new[] { "en-US" },
        ["French"] = new[] { "fr" },
        ["Italian"] = new[] { "it" },
        ["German"] = new[] { "de" },
        ["Spanish"] = new[] { "es" },
        ["Korean"] = new[] { "ko" },
        ["Simp_Chinese"] = new[] { "zh-Hans" },
        ["Trad_Chinese"] = new[] { "zh-Hant" },
    };

    public PokemonProjectSWSH(GameTitle title, string version, string baseFolder, string? patchFolder = null)
        : base(title, version, baseFolder, patchFolder)
    {

        AddReference("PersonalTable",
            new MatrixReader(@"romfs\bin\pml\personal\personal_total.bin", Version < new Version("1.2") ? 0xA8 : 0xB0),
            MarshalArray<Personal8>
            );

        AddReference("Wazaoboe",
            new MatrixReader(@"romfs\bin\pml\waza_oboe\wazaoboe_total.bin", 0x0104),
            data => ParseEnumerable(data, ParseLearnset)
            );

        AddReference("Tamagowaza",
            new MultiFileReader<byte[]>(@"romfs\bin\pml\tamagowaza", @"tamagowaza_*.bin"),
            data => ParseEnumerable(data, ParseTamagowaza)
            );

        AddReference("Waza",
            new MultiFileReader<byte[]>(@"romfs\bin\pml\waza\", "waza*.wazabin")
            );

        AddReference($"message", new MessageReader(@$"romfs\bin\message\", LanguageMaps));
        AddReference($"messagereference", new DataInfo(@"romfs\bin\message\English\common\{0}.dat"));
    }

    protected override MsgWrapper GetPreviewMsgWrapper(object[] args)
    {
        var name = (string)args[0];
        var info = GetData<DataInfo>("messagereference");
        var path = GetPath(info.RelatedPath.Replace("{0}", name));
        if (!File.Exists(path)) return null;
        var msg = new MsgDataV2(path);
        var wrapper = new MsgWrapper();
        wrapper.Load(msg, null);
        return wrapper;
    }



    public static LevelupMove[] ParseLearnset(BinaryReader br)
    {
        var list = new List<LevelupMove>();
        do
        {
            var move = br.ReadInt16();
            var level = br.ReadInt16();
            if (move == -1 && level == -1) continue;
            list.Add(new LevelupMove(move, level));
        } while (br.BaseStream.Position != br.BaseStream.Length);

        return list.ToArray();
    }

    public static Tuple<int, int[]> ParseTamagowaza(BinaryReader br)
    {
        var index = br.ReadInt16();
        var count = br.ReadInt16();

        var list = new List<int>();
        for (int i = 0; i < count; i++)
        {
            var move = br.ReadInt16();
            list.Add(move);
        }

        return new Tuple<int, int[]>(index, list.ToArray());
    }

    public static PokemonId[] RefsToIds(int count, int[] refs)
    {
        var ids = new List<PokemonId>();
        var formRefs = new Dictionary<int, int>();
        var formIncrements = new int[count + 1];
        for (var i = 0; i < refs.Length; i++)
        {
            if (i <= count)
            {
                ids.Add(new(i));
                formIncrements[i] = 1;
                if (refs[i] > 0)
                {
                    formRefs.Add(refs[i], i);
                }
            }
            else if (refs[i] == 0)
            {
                ids.Add(new());
            }
            else if (!formRefs.ContainsKey(refs[i]))
            {
                ids.Add(new());
            }
            else
            {
                var ri = formRefs[refs[i]];
                var c = formIncrements[ri]++;
                ids.Add(new(ri, c));
            }
        }

        return ids.ToArray();
    }

    public PokemonId[] GetPokemonIndexes()
    {
        var personals = GetData<Personal8[]>("PersonalTable").ToArray();
        var refs = personals.Select(x => (int)x.Form_Index).ToArray();
        var count = GetPokemonCount();
        var ids = RefsToIds(count, refs);
        //ids = ids.Select((x, i) => personals[i].Basic_hp == 0 ? "000.00" : x).ToArray();

        return ids.ToArray();
    }

    public PokemonId[] GetEggIndexes(int[] indexes)
    {
        var count = GetPokemonCount(egg: true);
        var ids = RefsToIds(count, indexes);
        return ids;
    }

    [Dump]
    public IEnumerable<string> DumpLearnsets()
    {
        var outputFolder = Path.Combine(OutputPath, "learnset");
        Directory.CreateDirectory(outputFolder);
        var indexes = GetPokemonIndexes();
        var personals = GetData<object[]>("PersonalTable").Cast<Personal8>().ToArray();
        var prefix = $"swordshield_{Version.Major}.{Version.Minor}";

        {
            var wazaoboe = GetData<LevelupMove[][]>("Wazaoboe").ToArray();
            var sb = new StringBuilder();
            for (var i = 0; i < wazaoboe.Length; i++)
            {
                var line = string.Join(',', wazaoboe[i].Select(x => $"{x.Level}:{x.Move}"));
                sb.AppendLine($"{indexes[i]}\t{line}");
            }
            var outputFile = Path.Combine(outputFolder, $"{prefix}.levelup.txt");
            File.WriteAllText(outputFile, sb.ToString());
            yield return outputFile;
        }

        {
            var tamagowaza = GetData<IEnumerable<object>>("Tamagowaza").Cast<Tuple<int, int[]>>().ToArray();
            var sb = new StringBuilder();
            var eggIndexes = GetEggIndexes(tamagowaza.Select(x => x.Item1).ToArray());
            for (var i = 0; i < eggIndexes.Length; i++)
            {
                var line = string.Join(",", tamagowaza[i].Item2);
                sb.AppendLine($"{eggIndexes[i]}\t{line}");
            }
            var outputFile = Path.Combine(outputFolder, $"{prefix}.egg.txt");
            File.WriteAllText(outputFile, sb.ToString());
            yield return outputFile;
        }

        {
            var sb = new StringBuilder();
            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Machine1, personals[i].Machine2, personals[i].Machine3, personals[i].Machine4);
                var data = TMList.Select((m, j) => new { m, j }).Where(x => tm[x.j]).Select(x => $"{x.m}:TM{x.j:00}");
                var line = string.Join(",", data);
                sb.AppendLine($"{indexes[i]}\t{line}");
            }
            var outputFile = Path.Combine(outputFolder, $"{prefix}.tm.txt");
            File.WriteAllText(outputFile, sb.ToString());
            yield return outputFile;
        }
        {
            var sb = new StringBuilder();
            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Record1, personals[i].Record2, personals[i].Record3, personals[i].Record4);
                var data = TRList.Select((m, j) => new { m, j }).Where(x => tm[x.j]).Select(x => $"{x.m}:TR{x.j:00}");
                var line = string.Join(",", data);
                sb.AppendLine($"{indexes[i]}\t{line}");
            }
            var outputFile = Path.Combine(outputFolder, $"{prefix}.tr.txt");
            File.WriteAllText(outputFile, sb.ToString());
            yield return outputFile;
        }

        {
            var sb = new StringBuilder();
            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Hiden_Machine);
                var data = TutorList.Select((m, j) => new { m, j }).Where(x => tm[x.j]).Select(x => $"{x.m}");
                var line = string.Join(",", data);
                sb.AppendLine($"{indexes[i]}\t{line}");
            }
            var outputFile = Path.Combine(outputFolder, $"{prefix}.tutor.txt");
            File.WriteAllText(outputFile, sb.ToString());
            yield return outputFile;
        }

        if (Version.Minor >= 2)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Tutor);
                var data = TutorList2.Select((m, j) => new { m, j }).Where(x => tm[x.j]).Select(x => $"{x.m}");
                var line = string.Join(",", data);
                sb.AppendLine($"{indexes[i]}\t{line}");
            }
            var outputFile = Path.Combine(outputFolder, $"{prefix}.tutor2.txt");
            File.WriteAllText(outputFile, sb.ToString());
            yield return outputFile;
        }
    }



}
