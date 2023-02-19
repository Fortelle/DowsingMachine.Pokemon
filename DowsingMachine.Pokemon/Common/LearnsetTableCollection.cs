using PBT.DowsingMachine.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace PBT.DowsingMachine.Pokemon.Common;

public class LearnsetTableCollection : IExportable
{
    public Dictionary<string, LearnsetTable> Tables { get; set; } = new();

    public string PokemonIdFormat { get; set; }

    public LearnsetTableCollection()
    {
    }

    public LearnsetTableCollection(string idFormat)
    {
        PokemonIdFormat = idFormat;
    }

    public void Add(string name, LearnsetTable table)
    {
        if (!string.IsNullOrEmpty(PokemonIdFormat))
        {
            table.PokemonIdFormat = PokemonIdFormat;
        }

        Tables.Add(name, table);
    }

    public Dictionary<string, string[]> GetPokemon(PokemonId id)
    {
        var dict = new Dictionary<string, string[]>();
        foreach(var (name, table) in Tables)
        {
            var entry = table.Find(x => x.Pokemon == id);
            if (entry == null || entry.Data.Length == 0) continue;
            dict.Add(name, entry.Data);
        }
        return dict;
    }

    public LearnsetDiff[] CompareWith(LearnsetTableCollection oldCollection, params string[] ignoreTables)
    {
        var dict = new List<LearnsetDiff>();

        foreach(var (key, newTable) in Tables)
        {
            if(ignoreTables.Contains(key))
            {
                continue;
            }
            if (!oldCollection.Tables.TryGetValue(key, out var oldTable))
            {
                continue;
            }
            if (oldTable.IsSpecialTable != newTable.IsSpecialTable)
            {
                continue;
            }

            foreach (var newEntry in newTable)
            {
                var pid = newEntry.Pokemon;
                if (!pid.IsValid) continue;
                var oldEntry = oldTable.FirstOrDefault(x => x.Pokemon == pid);
                if (oldEntry is null)
                {
                    continue;
                }

                var oldMoves = oldEntry.GetMoves();
                var newMoves = newEntry.GetMoves();
                var removedMoves = oldMoves.Where(x => !newMoves.Contains(x)).ToArray();
                var addedMoves = newMoves.Where(x => !oldMoves.Contains(x)).ToArray();

                if (removedMoves.Length > 0 || addedMoves.Length > 0)
                {
                    var diff = dict.FirstOrDefault(x => x.Pokemon == pid);
                    if(diff is null)
                    {
                        diff = new(pid);
                        dict.Add(diff);
                    }

                    diff.Add(key, removedMoves, addedMoves);
                }
            }
        }

        return dict.ToArray();
    }

    public bool Export(string folder)
    {
        foreach (var (key, table) in Tables)
        {
            var filename = Path.Combine(folder, key) + ".txt";
            table.SaveText(filename);
        }
        return true;
    }

    public static LearnsetTableCollection OpenDirectory(string path)
    {
        var files = Directory.GetFiles(path, "*.txt");
        var ltc = new LearnsetTableCollection();
        foreach(var file in files)
        {
            var key = Path.GetFileNameWithoutExtension(file);
            var table = LearnsetTable.Open(file);
            ltc.Add(key, table);
        }
        return ltc;
    }
}
