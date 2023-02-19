using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;

namespace PBT.DowsingMachine.Pokemon.Common;

public class LearnsetTable : List<LearnsetEntry>, IExportable
{
    public string PokemonIdFormat { get; set; }
    public bool IsSpecialTable { get; set; }

    public LearnsetTable()
    {
    }

    public LearnsetTable(bool isSpecialTalbe)
    {
        IsSpecialTable = isSpecialTalbe;
    }

    public void Add(PokemonId id)
    {
        this.Add(new LearnsetEntry(id, Array.Empty<string>()));
    }

    public void Add(PokemonId id, params int[] moves)
    {
        var data = moves.Select(x => x.ToString()).ToArray();
        this.Add(new LearnsetEntry(id, data));
    }

    public void Add(PokemonId id, params short[] moves)
    {
        var data = moves.Select(x => x.ToString()).ToArray();
        this.Add(new LearnsetEntry(id, data));
    }

    public void Add(PokemonId id, params string[] data)
    {
        this.Add(new LearnsetEntry(id, data));
    }

    public void SaveText(string path)
    {
        var format = string.IsNullOrEmpty(PokemonIdFormat) ? "{0:000}.{1:00}" : PokemonIdFormat;
        var sb = new StringBuilder();
        foreach(var entry in this)
        {
            sb.Append(entry.Pokemon.ToString(format));
            sb.Append('\t');
            sb.Append(string.Join(",", entry.Data));
            sb.AppendLine();
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, sb.ToString());
    }

    public void SaveJson(string path)
    {
        var list = new JsonArray();
        foreach(var (pm, data) in this)
        {
            var obj = new JsonObject
            {
                { "pokemon", pm.Number },
                { "form", pm.Form }
            };
            var moves = new JsonArray();
            foreach (var m in data)
            {
                var move = new JsonObject();
                var t = m.Split(":");
                move.Add("move", int.Parse(t[0]));
                if (path.Contains(".levelup."))
                {
                    move.Add("cond", "levelup");
                    move.Add("level", int.Parse(t[1]));
                }
                else
                {
                    move.Add("cond", t[1]);
                }
                moves.Add(move);
            }
            obj.Add("moves", moves);
            list.Add(obj);
        }
        JsonUtil.Serialize(path, list);
    }

    public bool Export(string path)
    {
        switch (Path.GetExtension(path))
        {
            case ".txt":
                SaveText(path);
                return true;
            case ".json":
                SaveJson(path);
                return true;
        }
        return false;
    }

    public static LearnsetTable Open(string filename)
    {
        var lt = new LearnsetTable();
        switch (Path.GetExtension(filename))
        {
            case ".txt":
                {
                    var lines = File.ReadAllLines(filename);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('\t');
                        lt.Add(PokemonId.Parse(parts[0]), parts[1].Split(','));
                    }
                }
                break;
            default:
                throw new NotSupportedException();
        }
        return lt;
    }
}
