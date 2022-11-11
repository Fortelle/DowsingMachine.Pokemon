using FlatSharp.Attributes;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

[FlatBufferTable]
public class FlatItem<T> where T : class
{
    [FlatBufferItem(0)] public byte Name { get; set; }
    public byte Hash { get; set; }
    [FlatBufferItem(1)] public T[] Items { get; set; }

    public static T[] Deserialize(byte[] data)
    {
        if (data == null) return null;
        var obj = FlatSharp.FlatBufferSerializer.Default.Parse<FlatItem<T>>(data);
        return obj.Items;
    }
}

