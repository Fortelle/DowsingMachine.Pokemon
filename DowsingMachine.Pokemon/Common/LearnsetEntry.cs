namespace PBT.DowsingMachine.Pokemon.Common;

public class LearnsetEntry
{
    public PokemonId Pokemon { get; set; }
    public string[] Data { get; set; }

    public LearnsetEntry(PokemonId pokemon, string[] data)
    {
        Pokemon = pokemon;
        Data = data;
    }

    public void Deconstruct(out PokemonId pokemon, out string[] data)
    {
        pokemon = Pokemon;
        data = Data;
    }

    public int[] GetMoves()
    {
        return Data
            .Select(x => x.Split(":", 2)[0])
            .Select(x => int.Parse(x))
            .Distinct()
            .ToArray();
    }
}
