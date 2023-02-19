using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Structures;
using PBT.DowsingMachine.Utilities;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public class PokemonProjectBDSP : PokemonProjectNS
{
    private static readonly Dictionary<string, (string, string)> LanguageMaps = new()
    {
        ["de"] = ("ge", "german"),
        ["en"] = ("en", "english"),
        ["es"] = ("sp", "spanish"),
        ["fr"] = ("fr", "french"),
        ["it"] = ("it", "italian"),
        ["ja-Jpan"] = ("jp", "jpn_kanji"),
        ["ja-Hrkt"] = ("jp", "jpn"),
        ["ko"] = ("ko", "korean"),
        ["zh-Hans"] = ("si", "simp_chinese"),
        ["zh-Hant"] = ("tr", "trad_chinese"),
    };

    public PokemonProjectBDSP() : base()
    {
        Resources.Add(new DataResource("PersonalTable")
        {
            Reference = new FileRef(@"assets\pml\data\PersonalTable.json"),
            Reader = new JsonReader("Personal"),
        });
        Resources.Add(new DataResource("WazaOboeTable")
        {
            Reference = new FileRef(@"assets\pml\data\WazaOboeTable.json"),
            Reader = new JsonReader("WazaOboe"),
        });
        Resources.Add(new DataResource("TamagoWazaTable")
        {
            Reference = new FileRef(@"assets\pml\data\TamagoWazaTable.json"),
            Reader = new JsonReader("Data"),
        });
        Resources.Add(new DataResource("WazaMachine")
        {
            Reference = new FileRef(@"assets\pml\data\ItemTable.json"),
            Reader = new JsonReader("WazaMachine")
                .Debug(x => x.AsArray().Select(y => new {
                    itemNo = (int)y["itemNo"],
                    itemName = GetString("ss", "itemname", (int)y["itemNo"]),
                    machineNo = (int)y["machineNo"],
                    wazaNo = (int)y["wazaNo"],
                    wazaName = GetString("ss", "wazaname", (int)y["wazaNo"]),
                })),
        });
        Resources.Add(new DataResource("WazaTable.Waza")
        {
            Reference = new FileRef(@"assets\pml\data\WazaTable.json"),
            Reader = new JsonReader("Waza"),
        });
        Resources.Add(new DataResource("WazaTable.Yubiwohuru")
        {
            Reference = new FileRef(@"assets\pml\data\WazaTable.json"),
            Reader = new JsonReader("Yubiwohuru"),
        });

        foreach (var (key, (value1, value2)) in LanguageMaps)
        {
            Resources.Add(new DataResource(key)
            {
                Reference = new FilesRef(@$"assets\format_msbt\{value1}\{value2}\*"),
                Reader = new DataReader<PathInfo[]>()
                    .Then(f => new TextTableCollection(f)),
            });
        }
        Resources.Add(new DataResource("message_preview")
        {
            Reference = new FileRef(@"assets\format_msbt\{0}\{1}\{1}_{2}_{3}.json"),
            Reader = new DataReader<string>()
                    .Then(f => new TextTable(f)),
            Browsable = false,
        });
    }

    public string GetString(string group, string filename, int value)
    {
        var strings = GetStrings(group, filename);
        return strings[value];
    }

    public string[] GetStrings(string group, string name)
    {
        var langcode = DowsingMachineApp.Config.Get<string>("PreviewLanguage");
        langcode = StringUtil.GetMostMatchedLangcode(LanguageMaps.Keys.ToArray(), langcode);
        if (string.IsNullOrEmpty(langcode)) langcode = "ja-Jpan";
        var values = LanguageMaps[langcode];
        return GetOrCreateCache($"{values.Item2}_{group}_{name}", () =>
        {
            var table = GetData<TextTable>("message_preview", new GetDataOptions()
            {
                ReferenceArguments = new[] { values.Item1, values.Item2, group, name },
                UseCache = false,
            });
            table.Load();
            return table.Entries.Select(x => x.Text).ToArray();
        });
    }

    [Test]
    public string[] test()
    {
        return GetStrings("dlp", "park_name");
    }

    [Data]
    public PokemonId[] GetIndexes()
    {
        var data = GetData<JsonArray>("PersonalTable");
        var query = data
            .Select((x, i) => new { id = (int)x["id"], monsno = (int)x["monsno"], order = i })
            .GroupBy(x => x.monsno)
            .SelectMany(g => g.Select((x, i) => new { x.id, x.monsno, x.order, form = i }))
            .OrderBy(x => x.order)
            .Select(x => new PokemonId(x.monsno, x.form))
            .ToArray();
        return query;
    }

    [Data(@"learnsets/")]
    public LearnsetTableCollection GetLearnsets()
    {
        var personalTable = GetData<JsonArray>("PersonalTable");
        var ids = GetData(GetIndexes);

        var collection = new LearnsetTableCollection(@"{0:0000}.{1:00}");

        {
            var wazaOboeTable = GetData<JsonArray>("WazaOboeTable");
            var lt = new LearnsetTable();
            foreach (var entry in wazaOboeTable)
            {
                var id = (int)entry["id"];
                var data = entry["ar"].AsArray()
                    .Select(x => (int)x)
                    .Chunk(2)
                    .Select(x => $"{x[1]}:{x[0]}")
                    .ToArray();
                lt.Add(ids[id], data);
            }
            collection.Add("levelup", lt);
        }

        {
            var wazaMachineTable = GetData<JsonArray>("WazaMachine")
                .Take(100)
                .ToArray();
            var lt = new LearnsetTable();
            foreach (var entry in personalTable)
            {
                var machines = new FlagArray((uint)entry["machine1"], (uint)entry["machine2"], (uint)entry["machine3"], (uint)entry["machine4"]);
                var data = machines.OfTrue(wazaMachineTable, (m, j) => $"{m["wazaNo"]}:TM{(int)m["machineNo"]:00}");
                var id = (int)entry["id"];
                lt.Add(ids[id], data);
            }
            collection.Add("tm", lt);
        }

        {
            var tutorList = new[] { 338, 307, 308, 434 };
            var lt = new LearnsetTable();
            foreach (var entry in personalTable)
            {
                var machines = new FlagArray((uint)entry["hiden_machine"]);
                var data = machines.OfTrue(tutorList);
                var id = (int)entry["id"];
                lt.Add(ids[id], data);
            }
            collection.Add("tutor", lt);
        }

        {
            var tamagoWazaTable = GetData<JsonArray>("TamagoWazaTable");
            var lt = new LearnsetTable();
            foreach (var entry in tamagoWazaTable)
            {
                var data = entry["wazaNo"].AsArray().Select(x => (int)x).ToArray();
                var id = new PokemonId((int)entry["no"], (int)entry["formNo"]);
                lt.Add(id, data);
            }
            collection.Add("egg", lt);
        }

        return collection;
    }


    [Action]
    public IEnumerable<string> DumpMessage()
    {
        foreach (var key in LanguageMaps.Keys)
        {
            var tc = GetData<TextTableCollection>(key);
            var outputFolder = Path.Combine(OutputFolder, key);
            foreach(var (name, table) in tc)
            {
                var m = Regex.Match(name, @"^(.+?)_(dlp|dp|ss)_(.+?)$");
                var filename = Path.Combine(outputFolder, "message", m.Groups[2].ToString(), $"{m.Groups[3]}.txt");
                table.Load();
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                File.WriteAllLines(filename, table.Entries.Select(x => x.Text));
                yield return filename;
            }
        }
    }


}
