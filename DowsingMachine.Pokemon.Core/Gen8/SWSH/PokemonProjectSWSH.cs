using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Utilities;
using System.Diagnostics;
using System.Text;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public class PokemonProjectSWSH : PokemonProjectNS, IPreviewString
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

    public PokemonProjectSWSH() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        var personalItemSize = Version switch
        {
            { Major: >= 1, Minor: >= 2 } => 0xB0,
            _ => 0xA8
        };

        Resources.Add(new DataResource("pokemon_personals")
        {
            Reference = new FileRef(@"romfs\bin\pml\personal\personal_total.bin"),
            Reader = new FileReader()
                .Then(br => br.ReadByteMatrix(personalItemSize))
                .Then(MarshalArray<Personal8>),
        });
        Resources.Add(new DataResource("pokemon_levelup_moves")
        {
            Reference = new FileRef(@"romfs\bin\pml\waza_oboe\wazaoboe_total.bin"),
            Reader = new FileReader()
                .Then(br => br.ReadByteMatrix(0x0104))
                .Then(ParseEnumerable(ReadPokemonLevelupMoves)),
        });
        Resources.Add(new DataResource("pokemon_egg_moves")
        {
            Reference = new FilesRef(@"romfs\bin\pml\tamagowaza\tamagowaza_*.bin"),
            Reader = new FilesReader<BinaryReader>()
                .Then(brs => brs.Select(ReadPokemonEggMoves).ToArray()),
        });


        Resources.Add(new DataResource("message")
        {
            Reference = new FilesRef(@"romfs\bin\message\*"),
            Reader = new MessageReader(LanguageMaps),
        });
        Resources.Add(new DataResource("message_preview")
        {
            Reference = new FileRef(@"romfs\bin\message\{0}\{1}\{2}.dat"),
            Reader = new DataReader<string>()
                .Then(path => new MsgWrapper(path, FileVersion.GenVIII)),
            PreviewArguments = new[]
            {
                "Simp_Chinese",
                "common",
                "monsname"
            }
        });

    }


    #region "Message"
    protected MsgFormatter MsgFormatter = new PokemonMsgFormatterV2();
    private MsgWrapper GetWrapper(string group, string filename, string langcode = null)
    {
        if (langcode == null)
        {
            langcode = DowsingMachineApp.Config.Get<string>("PreviewLanguage");
            langcode = StringUtil.GetMostMatchedLangcode(LanguageMaps.Values.SelectMany(x => x).ToArray(), langcode);
        }
        if (string.IsNullOrEmpty(langcode)) langcode = "ja-Jpan";
        var langname = LanguageMaps.First(x=>x.Value.Contains(langcode)).Key;

        var wrapper = GetOrCreateCache(langcode + "_" + filename, () => {
            var wr = GetData<MsgWrapper>("message_preview", new GetDataOptions()
            {
                UseCache = false,
                ReferenceArguments = new[] { langname, group, filename }
            });
            wr.Load();
            return wr;
        });
        return wrapper;
    }

    public string GetString(string group, string filename, object value)
    {
        var wrapper = GetWrapper(group, filename);
        if (wrapper == null) return "";

        var valuetext = value.ToString();
        if (int.TryParse(valuetext, out var index))
        {
            var entry = wrapper.TryGetEntry(index);
            if (entry == null || !entry.HasText) return $"({index})";
            return MsgFormatter.Format(entry[0], new());
        }
        else
        {
            var entry = wrapper.TryGetEntry(valuetext);
            if (entry == null) return $"(${value})";
            return MsgFormatter.Format(entry[0], new());
        }
    }

    public string[] GetStrings(string group, string filename)
    {
        var wrapper = GetWrapper(group, filename);
        Debug.Assert(wrapper != null);

        var options = new StringOptions(StringFormat.Plain, "");
        return wrapper.GetTextEntries()
            .Select(x => MsgFormatter.Format(x[0], options))
            .ToArray();
    }

    public string GetPreviewString(params object[] args)
    {
        if(args.Length == 2)
        {
            return GetString("common", $"{args[0]}", args[1]);
        }
        else if (args.Length == 3)
        {
            return GetString("common", $"{args[0]}", string.Format((string)args[1], args[2]));
        }
        throw new NotSupportedException();
    }
    #endregion


    private static LevelupMove[] ReadPokemonLevelupMoves(BinaryReader br)
    {
        var list = new List<LevelupMove>();
        while (true)
        {
            var move = br.ReadInt16();
            var level = br.ReadInt16();
            if (move == -1 && level == -1) break;
            list.Add(new LevelupMove(move, level));
        }
        return list.ToArray();
    }

    private (int Index, short[] Moves) ReadPokemonEggMoves(BinaryReader br)
    {
        var index = br.ReadInt16();
        var count = br.ReadInt16();
        var moves = br.ReadShorts(count);
        return (index, moves);
    }

    [Test]
    public (PokemonId, Personal8)[] GetKeyedPersonals()
    {
        return GetOrCreateCache(() =>
        {
            var personals = GetData<Personal8[]>("pokemon_personals");
            var formindexes = personals.Select(x => (int)x.Form_Index).ToArray();
            var maxnumber = GetPokemonCount();
            var ids = RefsToIds(formindexes, maxnumber);
            return ids.Zip(personals)
                .Select(x => (x.First, x.Second))
                .ToArray();
        });
    }

    [Test]
    public (PokemonId, short[])[] GetKeyedEggMoves()
    {
        return GetOrCreateCache(() =>
        {
            var tamagowaza = GetData<(int Index, short[] Moves)[]>("pokemon_egg_moves");
            var maxnumber = GetPokemonCount(egg: true);
            var formindexes = tamagowaza.Select(x => x.Index).ToArray();
            var ids = RefsToIds(formindexes, maxnumber);
            return ids.Zip(tamagowaza)
                .Select(x => (x.First, x.Second.Moves))
                .ToArray();
        });
    }

    private static PokemonId[] RefsToIds(int[] refs, int max)
    {
        var ids = new List<PokemonId>();
        var formRefs = new Dictionary<int, int>();
        var formIncrements = new int[max + 1];
        for (var i = 0; i < refs.Length; i++)
        {
            if (i <= max)
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


    [Data($"learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = GetKeyedPersonals();
        var wazaoboe = GetData<LevelupMove[][]>("pokemon_levelup_moves");
        var eggmoves = GetKeyedEggMoves();

        var collection = new LearnsetTableCollection("{0:000}.{1:000}");

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < wazaoboe.Length; i++)
            {
                var data = wazaoboe[i].Select(x => $"{x.Move}:{x.Level}");
                lt.Add(personals[i].Item1, data.ToArray());
            }
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, moves) in eggmoves)
            {
                lt.Add(id, moves);
            }
            collection.Add("egg", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var fa = new FlagArray(personal.Machine1, personal.Machine2, personal.Machine3, personal.Machine4);
                var data = fa.OfTrue(TMList, (m, j) => $"{m}:TM{j:00}");
                lt.Add(id, data);
            }
            collection.Add("tm", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var fa = new FlagArray(personal.Record1, personal.Record2, personal.Record3, personal.Record4);
                var data = fa.OfTrue(TRList, (m, j) => $"{m}:TR{j:00}");
                lt.Add(id, data);
            }
            collection.Add("tr", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var fa = new FlagArray(personal.Hiden_Machine);
                var data = fa.OfTrue(TutorList, (m, j) => $"{m}");
                lt.Add(id, data);
            }
            collection.Add("tutor", lt);
        }

        if (Version.Minor >= 2)
        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var fa = new FlagArray(personal.Tutor);
                var data = fa.OfTrue(TutorList2, (m, j) => $"{m}");
                lt.Add(id, data);
            }
            collection.Add("tutor2", lt);
        }

        return collection;
    }
    
    private string GetPokemonName(PokemonId id)
    {
        var monsname = GetString("common", "monsname", id.Number);
        var form = GetString("common", "zkn_form", $"ZKN_FORM_{id.Number:000}_{id.Form:000}");
        if (form != "") monsname += "(" + form + ")";
        return monsname;
    }


    [Action]
    public string CompareWithPreviousVersion()
    {
        using var old = DowsingMachineApp.FindProject<PokemonProjectSWSH>(s => s
            .Where(x => x.Version < Version)
            .MaxBy(x => x.Version)
            );

        if (old is null) return "";
        old.Active();
        old.BeginWork();

        var sb = new StringBuilder();

        {
            var oldPersonals = old.GetKeyedPersonals()
                .Where(x => x.Item1.IsValid)
                .Where(x => x.Item2.Enable > 0)
                .ToDictionary(x => x.Item1, x => x.Item2);
            var newPersonals = GetKeyedPersonals()
                .Where(x => x.Item1.IsValid)
                .Where(x => x.Item2.Enable > 0)
                .ToDictionary(x => x.Item1, x => x.Item2);

            var ch = new DictionaryComparer<PokemonId>()
            {
                IgnoreProperties = new[]
                {
                    "Form_Index",
                    "Tutor",
                    "Armor_zukan_no",
                    "Crown_zukan_no",
                },
                KeyToString = (x) => $"No.{x} {GetPokemonName(x)}",
            };
            sb.AppendLine("Pokemon:");
            sb.AppendLine(ch.Compare(oldPersonals, newPersonals));
        }

        {
            var oldLearnsets = old.DumpLearnsets();
            var newLearnsets = DumpLearnsets();
            var oldEnabled = old.GetKeyedPersonals()
                .Where(x => x.Item2.Enable > 0)
                .Select(x => x.Item1)
                .ToArray();
            var newEnabled = GetKeyedPersonals()
                .Where(x => x.Item2.Enable > 0)
                .Select(x => x.Item1)
                .ToArray();
            foreach (var table in oldLearnsets.Tables.Values)
            {
                table.RemoveAll(x => !oldEnabled.Contains(x.Pokemon));
            }
            foreach (var table in newLearnsets.Tables.Values)
            {
                table.RemoveAll(x => !newEnabled.Contains(x.Pokemon));
            }

            var diffs = oldLearnsets.CompareWith(newLearnsets);

            sb.AppendLine("Learnsets:");
            foreach (var diff in diffs)
            {
                sb.AppendLine($"No.{diff.Pokemon.Number:000} {(Monsname)diff.Pokemon.Number}");
                sb.AppendLine(diff.ToText());
            }
        }


        return sb.ToString();
    }

}
