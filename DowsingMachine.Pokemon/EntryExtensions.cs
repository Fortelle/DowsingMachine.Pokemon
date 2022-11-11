using PBT.DowsingMachine.Data;

namespace PBT.DowsingMachine.Pokemon;

public static class EntryExtensions
{
    public static T[] Marshal<T>(this IEnumerable<byte[]> source) where T : new()
    {
        return source.Select(MarshalUtil.Deserialize<T>).ToArray();
    }

}
