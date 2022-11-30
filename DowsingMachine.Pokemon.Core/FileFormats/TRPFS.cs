using FlatSharp.Attributes;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Utilities.Codecs;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class TRPFS : ICollectionArchive<byte[]>, ILargeArchive
{
    public const string Magic = "ONEPACK"; // 0x004b4341_50454E4F

    public TrpfsHeader Header { get; set; }
    public TrpfsInfo Info { get; set; }

    public byte[] this[int index] => GetData(index);
    public byte[] this[ulong hash] => GetData(Array.BinarySearch(Info.Hashes, hash));
    public byte[] this[string name] => GetData(Array.BinarySearch(Info.Hashes, FnvHash.Fnv1a_64(name)));

    private Stream Stream { get; set; }
    private BinaryReader Reader { get; set; }

    public TRPFS()
    {
    }

    public TRPFS(string path) => Open(path);

    public void Open(string path)
    {
        Stream = File.OpenRead(path);
        Reader = new BinaryReader(Stream);

        Load(Reader);
    }

    public void Open(byte[] data)
    {
        Stream = new MemoryStream(data);
        Reader = new BinaryReader(Stream);

        Load(Reader);
    }

    public void Open(Stream stream)
    {
        Stream = stream;
        Reader = new BinaryReader(stream);

        Load(Reader);
    }

    public IEnumerable<Entry<byte[]>> Entries
    {
        get
        {
            for(var i = 0; i < Info.Offsets.Length; i++)
            {
                yield return new Entry<byte[]>(GetData(i), i);
            }
        }
    }

    private byte[] GetData(int index)
    {
        if (index == -1) return null;

        var begin = (long)Info.Offsets[index];
        var end = index == Info.Offsets.Length - 1 
            ? Reader.BaseStream.Length 
            : (long)Info.Offsets[index + 1];
        var length = (int)(end - begin);

        Reader.BaseStream.Seek(begin, SeekOrigin.Begin);
        var data = Reader.ReadBytes(length);
        return data;
    }

    private byte[] ReadData(long begin, int length)
    {
        Reader.BaseStream.Seek(begin, SeekOrigin.Begin);
        var data = Reader.ReadBytes(length);
        return data;
    }

    private void Load(BinaryReader br)
    {
        Header = new TrpfsHeader(br);

        var infolength = Reader.BaseStream.Length - Header.InfoOffset;
        var infodata = ReadData(Header.InfoOffset, (int)infolength);
        Info = FlatBufferConverter.DeserializeFrom<TrpfsInfo>(infodata);

    }

    public void Dispose()
    {
        Stream?.Dispose();
        Reader?.Dispose();
    }

    public class TrpfsHeader
    {
        public const int Size = 0x10;

        public string Signature;
        public long InfoOffset;

        public TrpfsHeader(BinaryReader br) {
            Signature = new string(br.ReadChars(8));
            InfoOffset = br.ReadInt64();
        }
    }

    [FlatBufferTable]
    public class TrpfsInfo
    {
        [FlatBufferItem(0)] public ulong[] Hashes { get; set; }
        [FlatBufferItem(1)] public ulong[] Offsets { get; set; }
    }

}
