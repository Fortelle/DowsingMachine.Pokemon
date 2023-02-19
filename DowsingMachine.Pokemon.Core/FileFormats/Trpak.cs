using FlatSharp.Attributes;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using PBT.DowsingMachine.Utilities.Codecs;
using SoulsFormats;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

[FlatBufferTable]
public class Trpak
{
    [FlatBufferItem(0)] public ulong[] Hashes { get; set; }
    [FlatBufferItem(1)] public TrpakEntry[] Entries { get; set; }

    public byte[] this[int index] => GetData(index);
    public byte[] this[ulong hash] => GetData(Array.BinarySearch(Hashes, hash));
    public byte[] this[string name] => GetData(Array.BinarySearch(Hashes, FnvHash.Fnv1a_64(name)));

    private byte[] GetData(int index)
    {
        if (index == -1) return null;
        var entry = Entries[index];
        var data = entry.Data;
        if (entry.CompressionType > 0)
        {
            data = Decompress(data, (int)entry.DecompressedLength, entry.CompressionType);
        }
        return data;
    }

    public byte[][] GetAllData()
    {
        return Entries.Select((x, i) => GetData(i)).ToArray();
    }

    private static byte[] Decompress(byte[] compressedData, int decompressedLength, sbyte type)
    {
        switch (type)
        {
            case -1:
                return compressedData;
            case 1: // Zlib
                {
                    throw new NotImplementedException();
                }
            case 2: // Lz4
                {
                    var buff = new byte[decompressedLength];
                    K4os.Compression.LZ4.LZ4Codec.Decode(compressedData, buff);
                    return buff;
                }
            case >= 3 and <= 7: // Oodle
                {
                    var decompressedData = Oodle.Decompress(compressedData, decompressedLength);
                    return decompressedData;
                }
            default:
                throw new NotImplementedException();
        }
    }

    public static Trpak Load(string path)
    {
        return FlatBufferConverter.DeserializeFrom<Trpak>(path);
    }

    public static Trpak Load(byte[] data)
    {
        return FlatBufferConverter.DeserializeFrom<Trpak>(data);
    }

    [FlatBufferTable]
    public class TrpakEntry
    {
        [FlatBufferItem(0)] public uint A { get; set; }
        [FlatBufferItem(1)] public sbyte CompressionType { get; set; }
        [FlatBufferItem(2)] public byte C { get; set; }
        [FlatBufferItem(3)] public uint DecompressedLength { get; set; }
        [FlatBufferItem(4)] public byte[] Data { get; set; }
    }

}
