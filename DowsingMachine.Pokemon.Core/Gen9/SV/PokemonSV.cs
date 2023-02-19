using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using PBT.DowsingMachine.Pokemon.Core.Gen8;
using PBT.DowsingMachine.Projects;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PBT.DowsingMachine.Pokemon.Core.Gen9;

public class PokemonProjectSV : PokemonProjectTrinity, IPreviewString
{
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

    public PokemonProjectSV() : base()
    {
        Resources.Add(new DataResource("pokemon_personals")
        {
            Reference = new TrinityRef(@"avalon/data/personal_array.bin"),
            Reader = new DataReader<byte[]>()
                .Then(Metatable<PersonalSV>.Deserialize)
        });
        Resources.Add(new DataResource("moves")
        {
            Reference = new TrinityRef(@"avalon/data/waza_array.bin"),
            Reader = new DataReader<byte[]>()
                .Then(Metatable<WazaSV>.Deserialize)
        });

        Resources.Add(new DataResource("message_preview_dat")
        {
            Reference = new TrinityRef(@"message/dat/{0}/{1}/{2}.dat"),
            Reader = new DataReader<byte[]>()
                .Then(data => new MsgDataV2(data)),
            Previewable = false,
        });
        Resources.Add(new DataResource("message_preview_tbl")
        {
            Reference = new TrinityRef(@"message/dat/{0}/{1}/{2}.tbl"),
            Reader = new DataReader<byte[]>()
                .Then(data => new AHTB(data)),
            Previewable = false,
        });

    }

    [Test]
    public string WazaFlags()
    {
        var sb = new StringBuilder();
        var properties = typeof(WazaSV)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.Name.StartsWith("Flag_"))
            .ToArray();
        var wazadata = GetData<WazaSV[]>("waza");

        foreach (var prop in properties)
        {
            var wazalist = wazadata
                .Where(x => (bool)prop.GetValue(x))
                .Select(x => GetPreviewString("wazaname", x.WazaNo))
                .ToArray();
            sb.AppendLine(prop.Name);
            sb.AppendLine(string.Join(",", wazalist));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    #region "Message"
    protected MsgFormatter MsgFormatter= new PokemonMsgFormatterV2();
    private MsgWrapper GetWrapper(string folder, string filename, string langcode = null)
    {
        langcode ??= DowsingMachineApp.GetLangcode(LanguageMaps.Values.SelectMany(x => x).ToArray());
        var langname = LanguageMaps.First(x => x.Value.Contains(langcode)).Key;

        var wrapper = GetOrCreateCache(langcode + "_" + filename, () => {
            var msg = GetData<MsgDataV2>("message_preview_dat", new GetDataOptions()
            {
                UseCache = false,
                ReferenceArguments = new[] { langname, folder, filename }
            });
            var ahtb = GetData<AHTB>("message_preview_tbl", new GetDataOptions()
            {
                UseCache = false,
                ReferenceArguments = new[] { langname, folder, filename }
            });
            var wrapper = new MsgWrapper(msg, ahtb, filename, FileVersion.GenVIII, null);
            wrapper.Load();
            return wrapper;
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
            if (entry == null) return $"({index})";
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

        var options = new StringOptions(GFMSG.StringFormat.Plain, "");
        return wrapper.GetTextEntries()
            .Select(x => MsgFormatter.Format(x[0], options))
            .ToArray();
    }

    public string GetPreviewString(params object[] args)
    {
        return GetString("common", $"{args[0]}", args[1]);
    }
    #endregion


    [Test]
    public void Resize()
    {
        var fbs = new FolderBrowserDialog();
        if (fbs.ShowDialog() != DialogResult.OK) return;

        var inputFolder = fbs.SelectedPath;
        var outputFolder = Path.Combine(Path.GetDirectoryName(inputFolder), Path.GetFileName(inputFolder) + "_resized");
        Directory.CreateDirectory(outputFolder);

        var files = Directory.GetFiles(inputFolder, "*.png");
        foreach (var file in files)
        {
            using var bmp = Bitmap.FromFile(file);
            using var bmp2 = new Bitmap(bmp, new Size(1024, 536));
            var outputPath = Path.Combine(outputFolder, Path.GetFileName(file));
            bmp2.Save(outputPath);
        }

    }

    [Data("learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = GetData<PersonalSV[]>("pokemon_personals");
        var collection = new LearnsetTableCollection("{0:0000}.{1:00}");

        {
            var lt = new LearnsetTable();
            foreach (var pm in personals)
            {
                var id = new PokemonId(pm.NumForm.Number, pm.NumForm.Form);
                var data = pm.Waza_level.Select(x => $"{x.Waza}:{x.Level}").ToArray();
                lt.Add(id, data);
            }
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var pm in personals)
            {
                var id = new PokemonId(pm.NumForm.Number, pm.NumForm.Form);
                var data = pm.Waza_machine.Select(x => $"{x}").ToArray();
                lt.Add(id, data);
            }
            collection.Add("tm", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var pm in personals)
            {
                var id = new PokemonId(pm.NumForm.Number, pm.NumForm.Form);
                var data = pm.Waza_egg.Select(x => $"{x}").ToArray();
                lt.Add(id, data);
            }
            collection.Add("egg", lt);
        }
        
        {
            var lt = new LearnsetTable();
            foreach (var pm in personals)
            {
                var id = new PokemonId(pm.NumForm.Number, pm.NumForm.Form);
                var data = pm.Waza_tutor.Select(x => $"{x}").ToArray();
                lt.Add(id, data);
            }
            collection.Add("tutor", lt);
        }

        return collection;
    }

    private string GetPokemonName(PokemonId id)
    {
        var monsname = GetPreviewString("monsname", id.Number);
        var form = GetPreviewString("zkn_form", $"ZKN_FORM_{id.Number:000}_{id.Form:000}");
        if (form != "") monsname += "(" + form + ")";
        return monsname;
    }

    [Data("pokemon.md")]
    public string DumpPokemonInfo()
    {
        var personals = GetData<PersonalSV[]>("pokemon_personals");
        var learnsets = DumpLearnsets();
        var sb = new StringBuilder();
        foreach (var personal in personals)
        {
            if (!personal.Enable) continue;
            var pid = new PokemonId(personal.NumForm.Number, personal.NumForm.Form);
            sb.AppendLine($"No.{pid}  {GetPokemonName(pid)}");
            sb.AppendLine($"--------");
            sb.AppendLine($"- Base Stats: {personal.Basic.Hp} / {personal.Basic.Atk} / {personal.Basic.Def} / {personal.Basic.Spatk} / {personal.Basic.Spdef} / {personal.Basic.Agi} (Total: {personal.Basic.Hp + personal.Basic.Atk + personal.Basic.Def + personal.Basic.Spatk + personal.Basic.Spdef + personal.Basic.Agi})");
            sb.AppendLine($"- Types: {GetString("common", "typename", personal.Type1)} / {GetString("common", "typename", personal.Type2)}");
            sb.AppendLine($"- Abilities: {GetString("common", "tokusei", personal.Tokusei1)} / {GetString("common", "tokusei", personal.Tokusei2)} / {GetString("common", "tokusei", personal.Tokusei3)}");
            var ls = learnsets.GetPokemon(pid);
            foreach (var (name, moves) in ls)
            {
                sb.AppendLine($"- {name}:");
                foreach (var move in moves)
                {
                    sb.AppendLine($"  - {GetString("common", "wazaname", int.Parse(move.Split(':')[0]))}");
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    [Action]
    public string CompareWithSWSH()
    {
        var old = DowsingMachineApp.FindProject<PokemonProjectSWSH>(s => s
            .MaxBy(x => x.Version)
            );
        if (old is null) return "";
        old.BeginWork();

        var sb = new StringBuilder();

        {
            var oldLearnsets = old.DumpLearnsets();
            var newLearnsets = DumpLearnsets();
            var diffs = newLearnsets.CompareWith(oldLearnsets, "tm");
            foreach (var diff in diffs)
            {
                sb.AppendLine($"No.{diff.Pokemon} {GetPokemonName(diff.Pokemon)}");
                sb.AppendLine(diff.ToText());
            }
        }

        old.EndWork();
        return sb.ToString();
    }

    [Action]
    public string CompareWithPreviousVersion()
    {
        var old = DowsingMachineApp.FindProject<PokemonProjectSV>(s => s
            .Where(x => x != this && x.Version < Version)
            .MaxBy(x => x.Version)
            );
        if (old is null) return "";
        old.Active();
        old.BeginWork();

        var sb = new StringBuilder();

        {
            var oldPersonals = old.GetData<PersonalSV[]>("pokemon_personals").ToDictionary(pm => new PokemonId(pm.NumForm.Number, pm.NumForm.Form), pm => pm);
            var newPersonals = GetData<PersonalSV[]>("pokemon_personals").ToDictionary(pm => new PokemonId(pm.NumForm.Number, pm.NumForm.Form), pm => pm);

            var ch = new DictionaryComparer<PokemonId>()
            {
                KeyToString = (x) => $"No.{x} {GetPokemonName(x)}",
                IgnoreProperties = new[]
                {
                    "Waza_machine",
                    "Waza_egg",
                    "Waza_tutor",
                    "Waza_level",
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
            var movenames = GetStrings("common", "wazaname");
            sb.AppendLine("Learnsets");
            sb.AppendLine("----------");
            foreach (var diff in diffs)
            {
                sb.AppendLine($"No.{diff.Pokemon} {GetPokemonName(diff.Pokemon)}");
                sb.AppendLine(diff.ToText(movenames));
            }
        }

        {
            sb.AppendLine("Message");
            sb.AppendLine("----------");
            var oldMsg = old.DumpMessages();
            var newMsg = DumpMessages();
            var langcodes = LanguageMaps.Values.SelectMany(x => x).ToArray();
            var langcode = DowsingMachineApp.GetLangcode(langcodes);
            var ignores = LanguageMaps.Where(kv => !kv.Value.Contains(langcode)).Select(x => x.Key).ToArray();
            var diff = PokemonUtils.CompareGFMSG(oldMsg, newMsg, ignores);
            sb.AppendLine(diff);
        }

        return sb.ToString();
    }


    [Data]
    public MultilingualCollection DumpMessages()
    {
        var reg = new Regex(@"^arc/messagedat(.+?)(common|script)(.+?)\.(?:dat|tbl)\.trpak$");

        var lst = new List<(string Language, string Group, string Filename)>();

        {
            var trpfs = GetData<TRPFS>("data.trpfs");
            var trpfd = GetData<TRPFD>("data.trpfd");
            for (var i = 0; i < trpfd.PackNames.Length; i++)
            {
                var match = reg.Match(trpfd.PackNames[i]);
                if (!match.Success) continue;
                lst.Add((match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
            }
        }
        if (IsExtendable && OriginalProject != null)
        {
            var trpfs = OriginalProject.GetData<TRPFS>("data.trpfs");
            var trpfd = OriginalProject.GetData<TRPFD>("data.trpfd");
            for (var i = 0; i < trpfd.PackNames.Length; i++)
            {
                var match = reg.Match(trpfd.PackNames[i]);
                if (!match.Success) continue;
                var o = (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                if (lst.Any(x => x == o))
                {
                    continue;
                }
                lst.Add(o);
            }
        }

        var dict = new Dictionary<string, List<MsgWrapper>>();
        foreach (var (langname, group, name) in lst)
        {
            var langcodes = LanguageMaps[langname];

            var wrapper = new MsgWrapper(name, FileVersion.GenVIII, langcodes)
            {
                LazyLoad = (wr) =>
                {
                    var msg = GetData<MsgDataV2>("message_preview_dat", new GetDataOptions()
                    {
                        UseCache = false,
                        ReferenceArguments = new[] { langname, group, name }
                    });
                    var ahtb = GetData<AHTB>("message_preview_tbl", new GetDataOptions()
                    {
                        UseCache = false,
                        ReferenceArguments = new[] { langname, group, name }
                    });
                    wr.Load(msg, ahtb);
                }
            };

            if (!dict.TryGetValue(langcodes[0], out var wrappers))
            {
                wrappers = new List<MsgWrapper>();
                dict.Add(langname, wrappers);
            }
            wrappers.Add(wrapper);
        }
        var mc = new MultilingualCollection
        {
            Formatter = new PokemonMsgFormatterV2(),
            Wrappers = dict.ToDictionary(x => x.Key, x => x.Value.ToArray()),
        };
        return mc;
    }
}
