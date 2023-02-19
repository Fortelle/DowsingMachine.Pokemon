namespace PBT.DowsingMachine.Pokemon.Common;

public class FlagArray
{
    public bool[] Flags { get; set; }

    public FlagArray(params byte[] values)
    {
        Flags = ToBooleans(values);
    }

    public FlagArray(int count, params byte[] values)
    {
        Flags = ToBooleans(values).Take(count).ToArray();
    }

    public FlagArray(params uint[] values)
    {
        Flags = ToBooleans(values);
    }

    public TOut[] OfTrue<TOut>(Func<int, TOut> selector)
    {
        return Flags
            .Select((x, i) => new { Flag = x, Index = i })
            .Where(x => x.Flag)
            .Select(x => selector(x.Index))
            .ToArray();
    }

    public TOut[] OfTrue<TOut>(TOut[] values)
    {
        return values
            .Where((x, i) => Flags[i])
            .ToArray();
    }

    public TOut[] OfTrue<TIn, TOut>(TIn[] values, Func<TIn, TOut> selector)
    {
        return values
            .Where((x, i) => Flags[i])
            .Select(selector)
            .ToArray();
    }

    public TOut[] OfTrue<TIn, TOut>(TIn[] values, Func<TIn, int, TOut> selector)
    {
        return values
            .Select((x, i) => new { Value = x, Index = i, Flag = Flags[i] })
            .Where(x => x.Flag)
            .Select(x => selector(x.Value, x.Index))
            .ToArray();
    }

    private static bool[] ToBooleans(byte[] values)
    {
        var bools = values
            .Select(x => Convert.ToString(x, 2).PadLeft(8, '0').Reverse().Select(x => x == '1').ToArray())
            .SelectMany(x => x)
            .ToArray();
        return bools;
    }

    private static bool[] ToBooleans(uint[] values)
    {
        var bools = values
            .Select(x => BitConverter.GetBytes(x))
            .SelectMany(x => x)
            .Select(x => Convert.ToString(x, 2).PadLeft(8, '0').Reverse().Select(x => x == '1').ToArray())
            .SelectMany(x => x)
            .ToArray();
        return bools;
    }

}