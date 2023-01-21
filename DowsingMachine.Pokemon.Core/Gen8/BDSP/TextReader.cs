using PBT.DowsingMachine.Projects;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public class TextEntry
{
    public string Name { get; set; }
    public WordData[] Words { get; set; }
    public string Text { get; set; }
}

public class WordData
{
    public int patternID { get; set; }
    public int eventID { get; set; }
    public int tagIndex { get; set; }
    public double tagValue { get; set; }
    public string str { get; set; }
    public double strWidth { get; set; }
}

public class TextReader : DataReader<IEnumerable<Tuple<string, TextEntry[]>>>
{
    public string Lang { get; set; }
    public bool Format;

    public TextReader(string lang, string path) : base(path)
    {
        Lang = lang;
    }

    protected override IEnumerable<Tuple<string, TextEntry[]>> Open()
    {
        var path = Path.Combine(Project.As<PokemonProjectNS>().OriginalFolder, RelatedPath);
        var filelist = Directory.GetFiles(path, "*.json");
        foreach (var file in filelist)
        {
            var list = GetList(file, Format);
            yield return new Tuple<string, TextEntry[]>(file, list);
        }
    }

    public TextEntry[] GetList(string filename, bool format)
    {
        var text = File.ReadAllText(filename);
        var json = JsonNode.Parse(text);
        var labelDataArray = json["labelDataArray"] as JsonArray;
        var dict = labelDataArray.Select(x => new TextEntry()
        {
            Name = x["labelName"].ToString(),
            Text = Join(x["wordDataArray"].Deserialize<WordData[]>(), format)
        }).ToArray();
        return dict;
    }

    private string Join(WordData[] words, bool format)
    {
        var text = "";
        for (int i = 0; i < words.Length; i++)
        {
            switch (words[i].patternID)
            {
                case 0:
                case 7:
                    if (i > 0)
                    {
                        var addSpace = false;
                        if (words[i - 1].patternID == 2 || words[i - 1].patternID == 3 || words[i - 1].patternID == 4 || words[i - 1].patternID == 5)
                        {
                            addSpace = false;
                        }
                        else if (text.Length > 0 && words[i].str.Length > 0)
                        {
                            addSpace = true;
                        }

                        if (addSpace)
                        {
                            var c1 = text[^1];
                            var c2 = words[i].str[0];
                            if (Lang.StartsWith("ja"))
                            {
                                if (!char.IsPunctuation(c1) && !string.IsNullOrWhiteSpace($"{c1}") && !char.IsPunctuation(c2) && !string.IsNullOrWhiteSpace($"{c2}"))
                                {
                                    text += "　";
                                }
                            }
                            else if (Lang.StartsWith("zh"))
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

                    text += words[i].str;
                    break;

                case 5:
                    text += "<var>";
                    break;

                case 2:
                case 3:
                case 4:
                    if (!format)
                    {
                        text += words[i].str;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        if (format)
        {
            if (Lang.StartsWith("en") || Lang.StartsWith("fr") || Lang.StartsWith("de") || Lang.StartsWith("it") || Lang.StartsWith("es"))
            {
                text = text.Replace("‘", "'");
                text = text.Replace("’", "'");
                text = text.Replace("“", "\"");
                text = text.Replace("”", "\"");
            }
        }

        return text;
    }

}