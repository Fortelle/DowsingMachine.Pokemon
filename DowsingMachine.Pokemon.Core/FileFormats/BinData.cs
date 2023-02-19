using PBT.DowsingMachine.Data;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class BinData : ICollectionArchive<byte[]>
{
    public int Count => _count;
    public byte[] this[int index] => Data[index];
    public IEnumerable<Entry<byte[]>> AsEnumerable() => Data.Select((data,i) => new Entry<byte[]>(data, i));
    public IEnumerable<byte[]> Values => Data;

    public string Signature;
    private ushort _count;
    private uint[] _offsets;

    private byte[][] Data;

    public BinData()
    {
    }

    public BinData(string path) => Open(path);
    public BinData(byte[] data) => Open(data);

    public void Open(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        Load(reader);
    }

    public void Open(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        Load(reader);
    }

    public void Open(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        Load(reader);
    }

    private void Load(BinaryReader reader)
    {
        Signature = new string(reader.ReadChars(2));
        _count = reader.ReadUInt16();
        _offsets = Enumerable.Range(0, _count)
            .Select(i => reader.ReadUInt32())
            .ToArray();

        Data = new byte[_count][];
        for (int i = 0; i < _count; i++)
        {
            reader.BaseStream.Seek(_offsets[i], SeekOrigin.Begin);
            var length = i < _count - 1
                ? _offsets[i + 1] - _offsets[i]
                : reader.BaseStream.Length - _offsets[i];
            Data[i] = reader.ReadBytes((int)length);
        }
    }

}
