namespace PBT.DowsingMachine.Pokemon.Common;

public static class PokemonUtils
{
    public static readonly byte[] MagicOfMagic = new byte[] {
        0x2D,
        0x31,
        0x31,
        0x2D,
        0x41,
        0x41
    };

    public static bool[] ToBooleans(params byte[] values)
    {
        var bools = values
            .Select(x => Convert.ToString(x, 2).PadLeft(8, '0').Reverse().Select(x => x == '1').ToArray())
            .SelectMany(x => x)
            .ToArray();
        return bools;
    }

    public static bool[] ToBooleans(params uint[] values)
    {
        var bools = values
            .Select(x => BitConverter.GetBytes(x))
            .SelectMany(x => x)
            .Select(x => Convert.ToString(x, 2).PadLeft(8, '0').Reverse().Select(x => x == '1').ToArray())
            .SelectMany(x => x)
            .ToArray();
        return bools;
    }

    public static T[] MatchFlags<T>(T[] values, bool[] flags)
    {
        var list = new List<T>();
        for (var i = 0; i < flags.Length; i++)
        {
            if (flags[i])
            {
                list.Add(values[i]);
            }
        }
        return list.ToArray();
    }

    public static TOut[] MatchFlags<TIn, TOut>(TIn[] values, bool[] flags, Func<TIn, int, TOut> selector)
    {
        var list = new List<TOut>();
        for (var i = 0; i < flags.Length; i++)
        {
            if (flags[i])
            {
                var o = selector(values[i], i);
                list.Add(o);
            }
        }
        return list.ToArray();
    }

}
