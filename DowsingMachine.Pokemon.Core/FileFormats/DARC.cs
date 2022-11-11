using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Projects;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

// Directory ARChive
// https://www.3dbrew.org/wiki/DARC
public class DARC : ICollectionArchive<byte[]>, ILargeArchive
{
    protected Stream Stream { get; set; }
    protected BinaryReader Reader { get; set; }

    private DarcHeader Header { get; set; }
    private List<DarcEntry> Files { get; set; }
    private List<DarcName> Names { get; set; }

    public IEnumerable<Entry<byte[]>> Entries
    {
        get
        {
            for (var i = 0; i < Files.Count; i++)
            {
                if (!Files[i].IsFolder)
                {
                    var path = GetName(Files[i].NameOffset);
                    for (var j = i - 1; j > 0; j--)
                    {
                        if (Files[j].IsFolder)
                        {
                            path = GetName(Files[j].NameOffset) + "/" + path;
                            while (Files[j].DataOffset > 0)
                            {
                                j = (int)Files[j].DataOffset;
                                path = GetName(Files[j].NameOffset) + "/" + path;
                            }
                            break;
                        }
                    }
                    yield return new Entry<byte[]>(this[i], path, i);
                }
            }
        }
    }

    public byte[] this[int index]
    {
        get
        {
            var entry = Files[index];
            Debug.Assert(!entry.IsFolder);
            var offset = entry.DataOffset;
            Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return Reader.ReadBytes((int)entry.DataSize);
        }
    }

    public byte[] this[string name]
    {
        get
        {
            var nameitem = Names.FirstOrDefault(x => x.Name == name);
            var index = Files.FindIndex(x => x.NameOffset == nameitem.Offset);
            return this[index];
        }
    }


    public DARC()
    {
    }

    public DARC(byte[] data)
    {
        Open(data);
    }

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

    protected void Load(BinaryReader br)
    {

        Header = new DarcHeader(br);

        Files = new();
        var root = new DarcEntry(br);
        Files.Add(root);
        for (var i = 1; i < root.DataSize; i++)
        {
            Files.Add(new DarcEntry(br));
        }

        Names = new();
        var pos = br.BaseStream.Position;
        for (var i = 0; i < root.DataSize; i++)
        {
            var name = "";
            var offset = br.BaseStream.Position - pos;
            while (true)
            {
                var c = br.ReadUInt16();
                if (c == 0x0000) break;
                name += (char)c;
            }
            Names.Add(new DarcName()
            {
                Name = name,
                Offset = (uint)offset
            });
        }
    }

    private string GetName(uint offset)
    {
        return Names.FirstOrDefault(x => x.Offset == offset).Name;
    }

    public void Dispose()
    {
        Reader?.Dispose();
        Stream?.Dispose();
    }


    private class DarcHeader
    {
        public char[] Magic;
        public ushort Endianness;
        public ushort HeaderSize;
        public uint Version;
        public uint FileSize;

        public uint FileTableOffset;
        public uint FileTableSize;
        public uint FileDataOffset;

        public DarcHeader(BinaryReader br)
        {
            Magic = br.ReadChars(4);
            Debug.Assert(new string(Magic) == "darc");
            Endianness = br.ReadUInt16();
            HeaderSize = br.ReadUInt16();
            Version = br.ReadUInt32();
            FileSize = br.ReadUInt32();

            FileTableOffset = br.ReadUInt32();
            FileTableSize = br.ReadUInt32();
            FileDataOffset = br.ReadUInt32();
        }
    }

    private class DarcEntry
    {
        public uint NameOffset;
        public uint DataOffset;
        public uint DataSize;

        public bool IsFolder;

        public DarcEntry(BinaryReader br)
        {
            var v1 = br.ReadUInt32();
            var v2 = br.ReadUInt32();
            var v3 = br.ReadUInt32();

            NameOffset = v1 & 0xFFFFFF;
            IsFolder = ((v1 >> 24) & 0x1) == 0x1;
            DataOffset = v2;
            DataSize = v3;
        }
    }

    private struct DarcName
    {
        public uint Offset;
        public string Name;
    }

}
