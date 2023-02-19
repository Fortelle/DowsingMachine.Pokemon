using PBT.DowsingMachine.Data;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core.Colosseums;

public class FSYS : ICollectionArchive<string, byte[]>, IDisposable
{
    public const string Magic = "FSYS";

    private FsysHeader Header;
    private List<FsysInfo> Info;
    private List<string> Filenames;

    private BinaryReaderEx Reader { get; set; }

    public int Count => (int)Header.NumberOfEntries;

    public string[] Keys => Filenames.ToArray();

    public FSYS() { }
    public FSYS(string path) => Open(path);
    public FSYS(byte[] data) => Open(data);

    public void Open(string path)
    {
        Reader = new BinaryReaderEx(File.OpenRead(path)) { IsBigEndian = true };

        Load(Reader);
    }

    public void Open(byte[] data)
    {
        Reader = new BinaryReaderEx(new MemoryStream(data)) { IsBigEndian = true };

        Load(Reader);
    }

    public void Dispose()
    {
        Reader?.Dispose();
    }

    // https://github.com/PekanMmd/Pokemon-XD-Code/blob/3c2ce966e188910e118cb84c0924bd7239c5fdeb/Objects/file%20formats/XGFsys.swift
    private void Load(BinaryReaderEx br)
    {
        Header = new FsysHeader(br);

        br.BaseStream.Seek(Header.DetailsPointersListOffset, SeekOrigin.Begin);
        var detailOffsets = new List<uint>();
        for (var i = 0; i < Header.NumberOfEntries; i++)
        {
            detailOffsets.Add(br.ReadUInt32());
        }

        br.BaseStream.Seek(Header.FirstFileNamePointerOffset, SeekOrigin.Begin);
        Filenames = new List<string>();
        for (var i = 0; i < Header.NumberOfEntries; i++)
        {
            var n = "";
            while (true)
            {
                var c = br.ReadChar();
                if (c == 0x00) break;
                n += c;
            }
            Filenames.Add(n);
        }

        Info = new();
        for (var i = 0; i < Header.NumberOfEntries; i++)
        {
            br.BaseStream.Seek(detailOffsets[i], SeekOrigin.Begin);
            Info.Add(new FsysInfo(br));
        }
    }

    public byte[] this[int index]
    {
        get
        {
            Reader.BaseStream.Seek(Info[index].FileStartPointer, SeekOrigin.Begin);
            var isCompressed = Info[index].CompressedSize != Info[index].DecompressedSize;
            byte[] data;
            if (isCompressed)
            {
                var offset = Info[index].FileStartPointer + 0x10;
                var length = Info[index].CompressedSize - 0x10;
                Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                using var outputStream = new MemoryStream();
                Decode(Reader.BaseStream, (int)length, outputStream);
                data = outputStream.ToArray();
            }
            else
            {
                Reader.BaseStream.Seek(Info[index].FileStartPointer, SeekOrigin.Begin);
                data = Reader.ReadBytes((int)Info[index].DecompressedSize);
            }
            return data;
        }
    }

    public byte[] this[string name]
    {
        get
        {
            var index = Filenames.IndexOf(name);
            if (index == -1) return null;
            return this[index];
        }
    }

    public IEnumerable<Entry<byte[]>> AsEnumerable()
    {
        for(var i = 0; i < Count; i++)
        {
            yield return new Entry<byte[]>(this[i], Filenames[i], i);
        }
    }


    //LZSS
    private static void Decode(Stream inputStream, int length, Stream outputStream)
    {
        var EI = 12;
        var EJ = 4;
        var P = 2;

        var N = 1 << EI;
        var F = 1 << EJ;

        var rless = 2;
        var slidingWindowSize = N;
        var slidingWindow = Enumerable.Repeat((byte)0x00, slidingWindowSize).ToArray();

        var r = N - F - rless;

        int flags = 0;

        N -= 1;
        F -= 1;

        for (var i = 0; i < length; i++)
        {
            if ((flags & 0x100) == 0)
            {
                flags = inputStream.ReadByte();
                flags |= 0xff00;
            }
            if ((flags & 1) != 0)
            {
                var c = (byte)inputStream.ReadByte();
                outputStream.WriteByte(c);
                slidingWindow[r] = c;
                r = r + 1 & N;
            }
            else
            {
                int b1 = inputStream.ReadByte();
                int b2 = inputStream.ReadByte();
                b1 |= b2 >> EJ << 8;
                b2 = (b2 & F) + P;
                for (var k = 0; k <= b2; k++)
                {
                    var c = slidingWindow[b1 + k & N];
                    outputStream.WriteByte(c);
                    slidingWindow[r] = c;
                    r = r + 1 & N;
                }
            }
            flags >>= 1;
        }
    }


    private class FsysHeader
    {
        public char[] Signature;
        public uint Version;
        public uint GroupId;
        public uint NumberOfEntries;
        public uint HeaderSize;
        public uint FileSize;
        public uint DetailsPointersListOffset;
        public uint FirstFileNamePointerOffset;
        public uint FirstFileOffset;
        public uint FirstFileDetailsPointerOffset;

        public FsysHeader(BinaryReaderEx br)
        {
            Signature = br.ReadChars(4);
            Debug.Assert(new string(Signature) == Magic);
            Version = br.ReadUInt32();
            GroupId = br.ReadUInt32();
            NumberOfEntries = br.ReadUInt32();

            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();
            HeaderSize = br.ReadUInt32();

            FileSize = br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();

            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();

            DetailsPointersListOffset = br.ReadUInt32();
            FirstFileNamePointerOffset = br.ReadUInt32();
            FirstFileOffset = br.ReadUInt32();
            br.ReadUInt32();

            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();

            FirstFileDetailsPointerOffset = br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();
        }
    }

    private class FsysInfo
    {
        public ushort FileIdentifier;
        public ushort FileFormat;
        public uint FileStartPointer;
        public uint DecompressedSize;
        public uint CompressedSize;
        public uint FileDetailsFullFilename;
        public uint FileFormatIndex;
        public uint FileDetailsFilename;

        public FsysInfo(BinaryReaderEx br)
        {
            FileIdentifier = br.ReadUInt16();
            FileFormat = br.ReadUInt16();
            FileStartPointer = br.ReadUInt32();
            DecompressedSize = br.ReadUInt32();
            br.ReadUInt32();

            br.ReadUInt32();
            CompressedSize = br.ReadUInt32();
            br.ReadUInt32();
            FileDetailsFullFilename = br.ReadUInt32();

            FileFormatIndex = br.ReadUInt32();
            FileDetailsFilename = br.ReadUInt32();
        }
    }

}
