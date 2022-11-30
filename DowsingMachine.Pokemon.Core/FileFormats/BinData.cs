using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class BinData : ICollectionArchive<byte[]>
{
    public byte[] this[int index] => Data[index];
    public byte[] this[string name] => throw new NotImplementedException();
    public IEnumerable<Entry<byte[]>> Entries => Data.Select((data,i) => new Entry<byte[]>(data, i));

    public string Signature;
    public ushort Count;
    private uint[] Offsets;

    public byte[][] Data;

    public BinData()
    {
    }

    public BinData(string path) => Open(path);
    public BinData(byte[] data) => Open(data);
    public BinData(Stream stream) => Open(stream);

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
        Count = reader.ReadUInt16();
        Offsets = Enumerable.Range(0, Count)
            .Select(i => reader.ReadUInt32())
            .ToArray();

        Data = new byte[Count][];
        for (int i = 0; i < Count; i++)
        {
            reader.BaseStream.Seek(Offsets[i], SeekOrigin.Begin);
            var length = i < Count - 1
                ? Offsets[i + 1] - Offsets[i]
                : reader.BaseStream.Length - Offsets[i];
            Data[i] = reader.ReadBytes((int)length);
        }
    }

}
