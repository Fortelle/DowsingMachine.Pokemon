using PBT.DowsingMachine.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;

namespace PBT.DowsingMachine.Pokemon.Common;

public class LearnsetTable
{
    public record LearnsetEntry(PokemonId Pokemon, string[] Data);

    public List<LearnsetEntry> Entries { get; set; }

    public LearnsetTable()
    {
        Entries = new();
    }

    public void Add(PokemonId id)
    {
        Entries.Add(new LearnsetEntry(id, Array.Empty<string>()));
    }

    public void Add(PokemonId id, params int[] moves)
    {
        var data = moves.Select(x=>x.ToString()).ToArray();
        Entries.Add(new LearnsetEntry(id, data));
    }

    public void Add(PokemonId id, params string[] data)
    {
        Entries.Add(new LearnsetEntry(id, data));
    }

    public void Append(PokemonId id, params int[] moves)
    {
        var i = Entries.FindIndex(x => x.Pokemon == id);
        Debug.Assert(i >= 0);
        var data = Entries[i].Data.Concat(moves.Select(x => x.ToString())).ToArray();
        Entries[i] = new LearnsetEntry(id, data);
    }

    public void Save(string path, string format = "{0:000}.{1:00}")
    {
        var sb = new StringBuilder();
        foreach(var entry in Entries)
        {
            sb.Append(entry.Pokemon.ToString(format));
            sb.Append('\t');
            sb.Append(string.Join(",", entry.Data));
            sb.AppendLine();
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, sb.ToString());
    }

    public void SaveJson(string path, bool hasForm, bool hasCond)
    {
        var list = new JsonArray();
        foreach(var (pm, data) in Entries)
        {
            var obj = new JsonObject
            {
                { "pokemon", pm.Number }
            };
            if (hasForm) obj.Add("form", pm.Form);
            var moves = new JsonArray();
            foreach (var m in data)
            {
                if (hasCond)
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
                else
                {
                    moves.Add(JsonValue.Create(int.Parse(m)));
                }
            }
            obj.Add("moves", moves);
            list.Add(obj);
        }
        JsonUtil.Serialize(path, list);
    }
}
