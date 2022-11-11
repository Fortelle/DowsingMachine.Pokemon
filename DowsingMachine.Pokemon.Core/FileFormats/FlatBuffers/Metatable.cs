using FlatSharp.Attributes;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

[FlatBufferTable]
public class Metatable<T> where T : class
{
    [FlatBufferItem(0)] public T[] Tables { get; set; }

    public static T[]? Deserialize(byte[] data)
    {
        if (data == null) return null;
        var obj = FlatSharp.FlatBufferSerializer.Default.Parse<Metatable<T>>(data);
        return obj.Tables;
    }
}

