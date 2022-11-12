using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Projects;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

// Nitro ARChive
// https://github.com/HiroTDK/EditorNDS/blob/4df7b2b5a7bb9a6bffe419d33f61b3e1c3b71384/ROM%20Editor/FileHandlers/01%20NitroFiles/06%20NitroArchive.cs
// https://github.com/KillzXGaming/Switch-Toolbox/blob/12dfbaadafb1ebcd2e07d239361039a8d05df3f7/File_Format_Library/FileFormats/Archives/NARC.cs
public class NARC : ICollectionArchive<byte[]>, ILargeArchive
{
    public const string Magic = "NARC"; // 0x4E415243

    private NarcHeader Header;
    private FATB Fatb;
    private FNTB Fntb;
    private FIMG Fimg;

    public long DataOffset;

    public byte[] this[int index] => GetData(index);
    public byte[] this[string name] => throw new NotImplementedException();
    public IEnumerable<Entry<byte[]>> Entries => Enumerable.Range(0, Fatb.Count).Select(i => new Entry<byte[]>(this[i], i));

    private Stream Stream { get; set; }
    private BinaryReader Reader { get; set; }

    public NARC()
    {
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

    public void Open(Stream stream)
    {
        Stream = stream;
        Reader = new BinaryReader(stream);

        Load(Reader);
    }

    public void Dispose()
    {
        Reader?.Dispose();
        Stream?.Dispose();
    }

    protected void Load(BinaryReader br)
    {
        Header = new NarcHeader(br);
        Fatb = new FATB(br);
        Fntb = new FNTB(br);
        Fimg = new FIMG(br);

        DataOffset = br.BaseStream.Position;
    }

    public byte[] GetData(int index)
    {
        var entry = Fatb.Entries[index];
        var offset = entry.BeginOffset + DataOffset;
        Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        var length = entry.EndOffset - entry.BeginOffset;
        var data = Reader.ReadBytes((int)length);
        return data;
    }

    private class NarcHeader
    {
        public char[] Signature;
        public ushort Endianess;
        public ushort Version;
        public uint FileSize;
        public ushort HeaderSize;
        public ushort BlockCount;

        public NarcHeader(BinaryReader br)
        {
            Signature = br.ReadChars(4);
            Endianess = br.ReadUInt16();
            Version = br.ReadUInt16(); // 0x0100
            FileSize = br.ReadUInt32();
            HeaderSize = br.ReadUInt16(); // 0x10
            BlockCount = br.ReadUInt16(); // Always 0x0003 (BTAF, BTNF and GMIF)

            Debug.Assert(new string(Signature) == Magic);
        }
    }

    private class FATB // File Allocation TaBle
    {
        public const string Magic = "FATB";

        public char[] Signature;
        public uint Size;
        public ushort Count;
        public ushort Reserved;
        public FATB_Entry[] Entries;

        public FATB(BinaryReader br)
        {
            Signature = br.ReadChars(4);
            Size = br.ReadUInt32();
            Count = br.ReadUInt16();
            Reserved = br.ReadUInt16();
            Entries = new FATB_Entry[Count];
            for (int i = 0; i < Count; i++)
            {
                var begin = br.ReadUInt32();
                var end = br.ReadUInt32();
                Entries[i] = new FATB_Entry()
                {
                    BeginOffset = begin,
                    EndOffset = end,
                };
            }
        }
    }

    private struct FATB_Entry
    {
        public uint BeginOffset;
        public uint EndOffset;
    }

    private class FNTB // File Name TaBle
    {
        public char[] Signature;
        public uint Size;

        public FNTB_Entry[] DirectoryTable;
        public EntryNameTableEntry[] NameTable;

        public FNTB(BinaryReader br)
        {
            Signature = br.ReadChars(4);
            Size = br.ReadUInt32();

            var root = new FNTB_Entry(br);
            var dirCount = root.ParentId;
            var dirList = new List<FNTB_Entry>
            {
                root
            };
            for (int i = 1; i < dirCount; i++)
            {
                dirList.Add(new FNTB_Entry(br));
            }
            DirectoryTable = dirList.ToArray();

            var nameList = new List<EntryNameTableEntry>();
            while (nameList.Count(x => x.NameLength == 0) < dirCount - 1)
            {
                nameList.Add(new EntryNameTableEntry(br));
            }
        }
    }

    private struct FNTB_Entry
    {
        public uint EntryOffset;
        public ushort FirstFileId;
        public ushort ParentId; // or DirectoryCount for root

        public FNTB_Entry(BinaryReader br)
        {
            EntryOffset = br.ReadUInt32();
            FirstFileId = br.ReadUInt16();
            ParentId = br.ReadUInt16();
        }
    }

    private class EntryNameTableEntry
    {
        public int NameLength;
        public bool IsDirectory;

        public string? EntryName;
        public ushort? DirectoryId;

        public EntryNameTableEntry(BinaryReader br)
        {
            var v1 = br.ReadByte();
            NameLength = v1 & 0b011111111;
            IsDirectory = v1 >> 7 == 1;

            if (NameLength > 0) EntryName = new string(br.ReadChars(NameLength));
            if (IsDirectory) DirectoryId = br.ReadUInt16();
        }

    }

    private struct FIMG // File IMaGe
    {
        public char[] Signature;
        public uint Size;

        public FIMG(BinaryReader br)
        {
            Signature = br.ReadChars(4);
            Size = br.ReadUInt32();
        }
    }

}

