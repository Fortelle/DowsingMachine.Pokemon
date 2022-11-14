namespace PBT.DowsingMachine.Pokemon.Common;

public class LearnsetTableCollection
{
    public Dictionary<string, LearnsetTable> Tables { get; set; } = new();

    public LearnsetTableCollection()
    {
    }

    public void Add(string name, LearnsetTable table)
    {
        Tables.Add(name, table);
    }

    public void Output(string folder, string idformat, string prefix = "")
    {
        Directory.CreateDirectory(folder);
        foreach(var (key, table) in Tables)
        {
            var path = Path.Combine(folder, $"{prefix}{key}.txt");
            table.Save(path, idformat);
        }
    }

    public void CompareWith(LearnsetTableCollection oldCollection)
    {
        foreach(var (key, newTable) in Tables)
        {
            if (!oldCollection.Tables.TryGetValue(key, out var oldTable))
            {
                continue;
            }
            
            foreach(var newEntry in newTable)
            {
                var oldEntry = oldTable.FirstOrDefault(x => x.Pokemon == newEntry.Pokemon);\
                if (oldEntry is null)
                {
                    continue;
                }

            }
        }
    }

}
