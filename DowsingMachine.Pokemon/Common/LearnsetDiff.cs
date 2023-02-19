using System.Text;

namespace PBT.DowsingMachine.Pokemon.Common;

public class LearnsetDiff
{
    public PokemonId Pokemon { get; set; }
    public Dictionary<string, (int[], int[])> Diff { get; set; }

    public LearnsetDiff(PokemonId id)
    {
        Pokemon = id;
        Diff = new();
    }

    public void Add(string key, int[] removed, int[] added)
    {
        Diff.Add(key, (removed, added));
    }

    public string ToText(string[] moveNames = null)
    {
        moveNames ??= Enum.GetNames<Wazaname>();
        string toText(int[] moves, bool isAdd)
        {
            return string.Join(", ", moves.Select(x => (isAdd ? "[+]" : "[-]") + moveNames.ElementAtOrDefault(x)));
        }

        var sb = new StringBuilder();
        foreach(var (key, (removed, added)) in Diff)
        {
            sb.Append($"    {key}: ");
            sb.Append(toText(removed, false));
            sb.Append(removed.Length > 0 ? ", " : "");
            sb.Append(toText(added, true));
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
