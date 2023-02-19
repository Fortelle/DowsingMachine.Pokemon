using PBT.DowsingMachine.Data;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class SARC : ICollectionArchive<byte[]>, IDisposable
{
    private const string Magic = "SARC";

    public string Signature;
    public ushort HeaderSize;
    public ushort Endianness;
    public uint FileSize;
    public uint DataOffset;
    public ushort Version;
    public ushort Padding;

    public SFAT Sfat;
    public SFNT Sfnt;

    public int Count => Sfat.EntryCount;
    public byte[] this[int index] => GetData(index);
    public IEnumerable<Entry<byte[]>> AsEnumerable() => Enumerable.Range(0, Count)
        .Select(i => new Entry<byte[]>(this[i], GetFilename(i), i));
    public IEnumerable<byte[]> Values => Enumerable.Range(0, Count).Select(i => this[i]);

    private BinaryReader Reader { get; set; }


    public SARC()
    {
    }

    public SARC(string path) => Open(path);

    public void Open(string path)
    {
        Reader = new BinaryReader(File.OpenRead(path));

        Load();
    }

    public void Open(byte[] data)
    {
        Reader = new BinaryReader(new MemoryStream(data));

        Load();
    }


    private void Load()
    {
        Signature = new string(Reader.ReadChars(4));

        HeaderSize = Reader.ReadUInt16();
        Endianness = Reader.ReadUInt16();
        FileSize = Reader.ReadUInt32();
        DataOffset = Reader.ReadUInt32();
        Version = Reader.ReadUInt16();
        Padding = Reader.ReadUInt16();

        Sfat = new SFAT(Reader);
        Sfnt = new SFNT(Reader);
    }

    private string? GetFilename(int index)
    {
        var entry = Sfat.Entries[index];
        if (entry.FilenameAttributes == 0)
        {
            return null;
        }
        else
        {
            //Debug.Assert((entry.FilenameAttributes & 0xFF00000000) == 0x0100000000);
            var offset = (entry.FilenameAttributes & 0x00FFFFFF) * 4;
            Reader.BaseStream.Seek(Sfnt.Offset + offset, SeekOrigin.Begin);
            var name = "";
            while (true)
            {
                var c = (char)Reader.ReadByte();
                if (c == 0) break;
                name += c;
            }
            return name;
        }
    }

    private byte[] GetData(int index)
    {
        var entry = Sfat.Entries[index];
        var offset = entry.FileDataBegin;
        var length = entry.FileDataEnd - entry.FileDataBegin; // ???
        Reader.BaseStream.Seek(DataOffset + offset, SeekOrigin.Begin);
        var data = Reader.ReadBytes(length);
        return data;
    }

    public void Dispose()
    {
        Reader?.Dispose();
    }


    // File Allocation Table
    public class SFAT
    {
        public const string Magic = "SFAT";

        public string Signature;
        public ushort HeaderSize; // 0xC
        public ushort EntryCount; // up to 0x3FFF
        public uint HashMultiplier;
        public List<SFAT_Entry> Entries;

        public SFAT(BinaryReader br)
        {
            Signature = new string(br.ReadChars(4));

            HeaderSize = br.ReadUInt16();
            EntryCount = br.ReadUInt16();
            HashMultiplier = br.ReadUInt32();
            Entries = new List<SFAT_Entry>();

            for (int i = 0; i < EntryCount; i++)
            {
                Entries.Add(new SFAT_Entry(br));
            }
        }

        public uint CalculateHash(string filename)
        {
            uint hash = 0;
            foreach(var c in filename)
            {
                hash = hash * HashMultiplier + c;
            }
            return hash & 0xFFFFFFFF;
        }

    }

    public class SFAT_Entry
    {
        public uint FilenameHash;
        public int FilenameAttributes;
        public int FileDataBegin;
        public int FileDataEnd;

        public SFAT_Entry(BinaryReader br)
        {
            FilenameHash = br.ReadUInt32();
            FilenameAttributes = br.ReadInt32();
            FileDataBegin = br.ReadInt32();
            FileDataEnd = br.ReadInt32();
        }
    }

    // File Name Table
    public class SFNT
    {
        public const string Magic = "SFNT";

        public string Signature;
        public ushort HeaderSize; // 0x8
        public ushort Padding;

        public long Offset;

        public SFNT(BinaryReader br)
        {
            Signature = new string(br.ReadChars(4));
            HeaderSize = br.ReadUInt16();
            Padding = br.ReadUInt16();

            Offset = br.BaseStream.Position;
        }
    }
}

