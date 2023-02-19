using PBT.DowsingMachine.Structures;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public record TextEntry(string Label, string Text);

public class TextTable
{
    public bool Name { get; set; }
    public TextEntry[] Entries { get; set; }
    public bool Format { get; set; } = false;
    public string Langcode { get; set; } = "";

    private string Filename;

    public TextTable(string filename)
    {
        Filename = filename;
    }

    public void Load()
    {
        if (Entries != null) return;
        var text = File.ReadAllText(Filename);
        var json = JsonNode.Parse(text);
        var labelDataArray = json["labelDataArray"] as JsonArray;
        Entries = labelDataArray.Select(x =>
        {
            var label = x["labelName"].ToString();
            var text = Join(x["wordDataArray"].Deserialize<WordData[]>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true } ));
            return new TextEntry(label, text);
        }).ToArray();
    }
    
    private string Join(WordData[] words)
    {
        var text = "";
        for (int i = 0; i < words.Length; i++)
        {
            switch (words[i].PatternID)
            {
                case 0:
                case 7:
                    if (i > 0)
                    {
                        var addSpace = false;
                        if (words[i - 1].PatternID == 2 || words[i - 1].PatternID == 3 || words[i - 1].PatternID == 4 || words[i - 1].PatternID == 5)
                        {
                            addSpace = false;
                        }
                        else if (text.Length > 0 && words[i].Str.Length > 0)
                        {
                            addSpace = true;
                        }

                        if (addSpace)
                        {
                            var c1 = text[^1];
                            var c2 = words[i].Str[0];
                            if (Langcode.StartsWith("ja"))
                            {
                                if (!char.IsPunctuation(c1) && !string.IsNullOrWhiteSpace($"{c1}") && !char.IsPunctuation(c2) && !string.IsNullOrWhiteSpace($"{c2}"))
                                {
                                    text += "　";
                                }
                            }
                            else if (Langcode.StartsWith("zh"))
                            {
                            }
                            else
                            {
                                if (!char.IsPunctuation(c1) && !string.IsNullOrWhiteSpace($"{c1}") && !char.IsPunctuation(c2) && !string.IsNullOrWhiteSpace($"{c2}"))
                                {
                                    text += " ";
                                }
                            }
                        }
                    }

                    text += words[i].Str;
                    break;

                case 5:
                    text += "<var>";
                    break;

                case 2:
                case 3:
                case 4:
                    if (!Format)
                    {
                        text += words[i].Str;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        if (Format)
        {
            if (Langcode.StartsWith("en") || Langcode.StartsWith("fr") || Langcode.StartsWith("de") || Langcode.StartsWith("it") || Langcode.StartsWith("es"))
            {
                text = text.Replace("‘", "'");
                text = text.Replace("’", "'");
                text = text.Replace("“", "\"");
                text = text.Replace("”", "\"");
            }
        }

        return text;
    }

    private class WordData
    {
        public int PatternID { get; set; }
        public int EventID { get; set; }
        public int TagIndex { get; set; }
        public double TagValue { get; set; }
        public string Str { get; set; }
        public double StrWidth { get; set; }
    }
}

public class TextTableCollection : Dictionary<string, TextTable>
{
    public TextTableCollection(PathInfo[] files)
    {
        foreach (var file in files)
        {
            Add(Path.GetFileNameWithoutExtension(file.FullPath), new TextTable(file.FullPath));
        }
    }

}


//public class TextReader : DataReader<IEnumerable<Tuple<string, TextEntry[]>>>
//{
//    public string Lang { get; set; }
//    public bool Format;

//    public TextReader(string lang, string path)
//    {
//        Lang = lang;
//    }

//    //protected override IEnumerable<Tuple<string, TextEntry[]>> Open()
//    //{
//    //    var path = Path.Combine(Project.As<PokemonProjectNS>().OriginalFolder, RelatedPath);
//    //    var filelist = Directory.GetFiles(path, "*.json");
//    //    foreach (var file in filelist)
//    //    {
//    //        var list = GetList(file, Format);
//    //        yield return new Tuple<string, TextEntry[]>(file, list);
//    //    }
//    //}

//    public TextEntry[] GetList(string filename, bool format)
//    {
//        var text = File.ReadAllText(filename);
//        var json = JsonNode.Parse(text);
//        var labelDataArray = json["labelDataArray"] as JsonArray;
//        var dict = labelDataArray.Select(x => new TextEntry()
//        {
//            Name = x["labelName"].ToString(),
//            Text = Join(x["wordDataArray"].Deserialize<WordData[]>(), format)
//        }).ToArray();
//        return dict;
//    }

//}