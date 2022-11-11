using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;
using System.Text;
using System.Text.Json.Nodes;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public class PokemonProjectBDSP : PokemonProjectNS
{
    private static readonly Dictionary<string, string> LanguageMaps = new()
    {
        ["de"] = "ge/german",
        ["en"] = "en/english",
        ["es"] = "sp/spanish",
        ["fr"] = "fr/french",
        ["it"] = "it/italian",
        ["ja-Jpan"] = "jp/jpn_kanji",
        ["ja-Hrkt"] = "jp/jpn",
        ["ko"] = "ko/korean",
        ["zh-Hans"] = "si/simp_chinese",
        ["zh-Hant"] = "tr/trad_chinese",
    };

    public PokemonProjectBDSP(GameTitle title, string version, string baseFolder, string patchFolder = "")
        : base(title, version, baseFolder, patchFolder)
    {
        AddReference("PersonalTable",
            new JsonReader(@"assets\pml\data\PersonalTable.json", "Personal")
            );

        AddReference("PokemonInfo",
            new JsonReader(@"assets\md\pokemondata\PokemonInfo.json", "Catalog")
            );
        AddReference("WazaOboeTable",
            new JsonReader(@"assets\pml\data\WazaOboeTable.json", "WazaOboe")
            );
        AddReference("TamagoWazaTable",
            new JsonReader(@"assets\pml\data\TamagoWazaTable.json", "Data")
            );
        AddReference("WazaMachine",
            new JsonReader(@"assets\pml\data\ItemTable.json", "WazaMachine")
            );
        AddReference("WazaTable.Waza",
            new JsonReader(@"assets\pml\data\WazaTable.json", "Waza")
            );
        AddReference("WazaTable.Yubiwohuru",
            new JsonReader(@"assets\pml\data\WazaTable.json", "Yubiwohuru")
            );

        foreach (var kv in LanguageMaps)
        {
            AddReference(kv.Key,
                new TextReader(kv.Key, @$"assets\format_msbt\{kv.Value}\")
                );
        }

    }

    public string[] GetIndexes()
    {
        var data = GetData<JsonArray>("PersonalTable");
        var query = data
            .Select((x, i) => new { id = (int)x["id"], monsno = (int)x["monsno"], order = i })
            .GroupBy(x => x.monsno)
            .SelectMany(g => g.Select((x, i) => new { x.id, x.monsno, x.order, form = i }))
            .OrderBy(x => x.order)
            .Select(x => $"{x.monsno:0000}.{x.form:00}")
            ;
        return query.ToArray();
    }

    [Dump]
    public IEnumerable<string> DumpLearnsets()
    {
        var pokemonIndexes = GetIndexes();
        var path = OutputPath + "/" + "Learnsets/";
        Directory.CreateDirectory(path);

        var personalTable = GetData<JsonArray>("PersonalTable");
        //var wazaMachineTable = GetReader<JsonArrayReader>("WazaMachine").Read();
        //var wazaMachine = wazaMachineTable.Select(x => (int)x["wazaNo"]).ToArray();
        //var sbMachine = new StringBuilder();
        //foreach (var entry in personalTable)
        //{
        //    var machines = new[] {
        //        (uint)entry["machine1"],
        //        (uint)entry["machine2"],
        //        (uint)entry["machine3"],
        //        (uint)entry["machine4"],
        //    };
        //    var bools = machines
        //        .Select(x => BitConverter.GetBytes(x))
        //        .SelectMany(x => x)
        //        .Select(x => Convert.ToString(x, 2).PadLeft(8, '0').Reverse().Select(x => x == '1').ToArray())
        //        .SelectMany(x => x)
        //        .ToArray();
        //    var lines = Enumerable.Range(0, 100)
        //        .Select(x => bools[x])
        //        .Select((x, i) => x ? $"{wazaMachine[i]}:TM{i + 1:00}" : null)
        //        .Where(x => x != null)
        //        .ToArray();
        //    var line = string.Join(',', lines);
        //    var id = (int)entry["id"];
        //    sbMachine.AppendLine($"{pokemonIndexes[id]}\t{line}");
        //    File.WriteAllText(path + "/" + "brilliantdiamondshiningpearl.tm.txt", sbMachine.ToString());
        //}

        var tutorList = new[] { 338, 307, 308, 434 };
        var sbTutor = new StringBuilder();
        foreach (var entry in personalTable)
        {
            var machines = new[] {
                (uint)entry["hiden_machine"],
            };
            var bools = machines
                .Select(x => BitConverter.GetBytes(x))
                .SelectMany(x => x)
                .Select(x => Convert.ToString(x, 2).PadLeft(8, '0').Reverse().Select(x => x == '1').ToArray())
                .SelectMany(x => x)
                .ToArray();
            var lines = Enumerable.Range(0, 8)
                .Select(x => bools[x])
                .Select((x, i) => x ? $"{tutorList[i]}" : null)
                .Where(x => x != null)
                .ToArray();
            var line = string.Join(',', lines);
            var id = (int)entry["id"];
            sbTutor.AppendLine($"{pokemonIndexes[id]}\t{line}");
            File.WriteAllText(path + "/" + "brilliantdiamondshiningpearl.tutor.txt", sbTutor.ToString());
            yield return path + "/" + "brilliantdiamondshiningpearl.tutor.txt";
        }

        //var tamagoWazaTable = GetReader<JsonArrayReader>("TamagoWazaTable").Read();
        //var sbTamago = new StringBuilder();
        //foreach (var entry in tamagoWazaTable)
        //{
        //    var name = $"{(int)entry["no"]:0000}.{(int)entry["formNo"]:00}";
        //    var line = string.Join(',', entry["wazaNo"].AsArray().Select(x => (int)x));
        //    sbTamago.AppendLine($"{name}\t{line}");
        //}
        //File.WriteAllText(path + "/" + "brilliantdiamondshiningpearl.egg.txt", sbTamago.ToString());

        //var wazaOboeTable = GetReader<JsonArrayReader>("WazaOboeTable").Read();
        //var sbLevelup = new StringBuilder();
        //foreach(var entry in wazaOboeTable)
        //{
        //    var id = (int)entry["id"];
        //    var ar = entry["ar"].AsArray().Select(x => (int)x).Chunk(2);
        //    var line = string.Join(',', ar.Select(x => $"{x[1]}:{x[0]}"));
        //    sbLevelup.AppendLine($"{pokemonIndexes[id]}\t{line}");
        //}
        //File.WriteAllText(path + "/" + "brilliantdiamondshiningpearl.levelup.txt", sbLevelup.ToString());
    }


    //[Extraction]
    //public IEnumerable<string> ExtractTexts()
    //{
    //foreach (var kv in LanguageMaps)
    //{
    //    var reader = GetReference(kv.Key);
    //    var data = reader.Read(true);

    //    var path = OutputPath + "/" + "Message/Formatted/" + kv.Key + "/";
    //    Directory.CreateDirectory(path);
    //    //Directory.CreateDirectory(path + "/dlp");
    //    //Directory.CreateDirectory(path + "/dp");
    //    //Directory.CreateDirectory(path + "/ss");

    //    foreach (var f in data)
    //    {
    //        var lines = f.Item2.Select(x => x.Text);

    //        var m = Regex.Match(Path.GetFileName(f.Item1), "^(.+?)_(dlp|dp|ss)_(.+?).json$");
    //        var filename = path + $"{m.Groups[2]}_{m.Groups[3]}.txt";

    //        Serializer.WriteLF(filename, lines);
    //        yield return filename;
    //    }
    //}
    //}
}
