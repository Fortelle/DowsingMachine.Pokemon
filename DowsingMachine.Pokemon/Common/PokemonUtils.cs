using GFMSG;
using System.Text;

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



    public static string CompareGFMSG(MultilingualCollection oldCollection, MultilingualCollection newCollection, params string[] ignoreTables)
    {
        var sb = new StringBuilder();

        foreach (var (langcode, newWrappers) in newCollection.Wrappers)
        {
            var options = new StringOptions(StringFormat.Markup);
            if (ignoreTables.Contains(langcode))
            {
                continue;
            }
            if (!oldCollection.Wrappers.TryGetValue(langcode, out var oldWrappers))
            {
                continue;
            }

            foreach (var oldWrapper in oldWrappers)
            {
                var newWrapper = newWrappers.FirstOrDefault(x => x.Name == oldWrapper.Name);
                if (newWrapper == null)
                {
                    sb.AppendLine($"{oldWrapper.Name}: Removed.");
                    sb.AppendLine();
                    continue;
                }

                var sb2 = new StringBuilder();
                oldWrapper.Load();
                newWrapper.Load();
                foreach (var oldEntry in oldWrapper.Entries)
                {
                    if (!oldEntry.HasText) continue;

                    var newEntry = newWrapper.Entries.FirstOrDefault(x => x.Name == oldEntry.Name);
                    if (newEntry == null)
                    {
                        var oldText = newCollection.Formatter.Format(oldEntry[0], options with { LanguageCode = oldEntry[0].Language });
                        sb2.AppendLine($"  {oldEntry.Name}:");
                        sb2.AppendLine($"    Removed: {oldText}");
                        continue;
                    }

                    var langcount = Math.Min(oldEntry.Sequences.Count, newEntry.Sequences.Count);
                    for (var i = 0; i < langcount; i++)
                    {
                        var oldText = oldCollection.Formatter.Format(oldEntry[i], options with { LanguageCode = oldEntry[i].Language });
                        var newText = newCollection.Formatter.Format(newEntry[i], options with { LanguageCode = oldEntry[i].Language });

                        if (oldText != newText)
                        {
                            var entryname = oldEntry.Name;
                            if (langcount > 1) entryname += $"[{i}]";
                            sb2.AppendLine($"  {entryname}:");
                            sb2.AppendLine($"    Old: {oldText}");
                            sb2.AppendLine($"    New: {newText}");
                        }
                    }
                }
                foreach (var newEntry in newWrapper.Entries)
                {
                    if (!newEntry.HasText) continue;
                    if (!oldWrapper.Entries.Any(x => x.Name == newEntry.Name))
                    {
                        var newText = newCollection.Formatter.Format(newEntry[0], options);
                        sb2.AppendLine($"  {newEntry.Name}:");
                        sb2.AppendLine($"    Added: {newText}");
                    }
                }

                if (sb2.Length > 0)
                {
                    sb.AppendLine($"{oldWrapper.Name}:");
                    sb.AppendLine(sb2.ToString());
                }
            }
            foreach (var newWrapper in newWrappers)
            {
                if (!oldWrappers.Any(x => x.Name == newWrapper.Name))
                {
                    sb.AppendLine($"{newWrapper.Name}: Added.");
                    sb.AppendLine();
                }
            }
        }

        return sb.ToString();
    }

}
