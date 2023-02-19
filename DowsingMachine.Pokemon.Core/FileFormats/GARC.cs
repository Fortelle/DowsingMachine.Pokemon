using PBT.DowsingMachine.Data;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class GARC : ICollectionArchive<byte[]>, IDisposable
{
    public const string Magic = "GARC"; // 0x4E415243

    private GarcHeader Header;
    private FATO Fato;
    private FATB Fatb;
    private FIMG Fimg;

    private BinaryReader Reader { get; set; }

    public int Count { get; set; }
    public byte[] this[int index] => GetData(index);
    public IEnumerable<Entry<byte[]>> AsEnumerable()
    {
        int c = 0;

        for (int i = 0; i < Fatb.FileCount; i++)
        {
            for (int j = 0; j < Fatb.Files[i].Variations.Length; j++)
            {
                if (Fatb.Files[i].Variations[j] != null)
                {
                    var data = GetData(i, j);
                    yield return new Entry<byte[]>(data, c++);
                }
            }
        }
    }

    public GARC()
    {
    }

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
        Header = new(Reader);
        Fato = new (Reader);
        Fatb = new (Reader);
        Fimg = new (Reader);

        Count = Fatb.Files.Sum(x => x.Variations.Length);
    }

    private byte[]? GetData(int fileIndex)
    {
        return GetData(fileIndex, 0);
    }

    private byte[]? GetData(int fileIndex, int langIndex)
    {
        var file = Fatb.Files[fileIndex];
        var variation = file.Variations[langIndex];

        Reader.BaseStream.Seek(Header.TotalBlockSize + variation.Begin, SeekOrigin.Begin);
        var buffer = new byte[variation.Length];
        Reader.Read(buffer, 0, variation.Length);

        return buffer;
    }

    public void Dispose()
    {
        Reader?.Dispose();
    }


    public class GarcHeader
    {
        public char[] Signature;
        public uint BlockSize;

        public ushort ByteOrder; // 0xFFFE
        public byte MinorVersion;
        public byte MajorVersion;
        public ushort BlockCount; // 0x04 (header, fato, fatb, fimg)
        public uint TotalBlockSize;
        public uint FileSize;
        public uint MaxDataSize;

        // for version >= 5
        public uint MaxRealDataSize;
        public uint Aligument;

        public GarcHeader(BinaryReader br)
        {
            Signature = br.ReadChars(4);
            Debug.Assert(new string(Signature.Reverse().ToArray()) == Magic);
            BlockSize = br.ReadUInt32();

            ByteOrder = br.ReadUInt16();
            MinorVersion = br.ReadByte();
            MajorVersion = br.ReadByte();

            BlockCount = br.ReadUInt16();
            _ = br.ReadUInt16(); // padding

            TotalBlockSize = br.ReadUInt32();
            FileSize = br.ReadUInt32();

            MaxDataSize = br.ReadUInt32();

            if (MajorVersion == 4)
            {
                Aligument = 4;
            }
            else if (MajorVersion >= 5)
            {
                MaxRealDataSize = br.ReadUInt32();
                Aligument = br.ReadUInt32();
            }
        }
    }

    public class FATO // File Allocation Table Offsets
    {
        public char[] Signature;
        public uint BlockSize;

        public ushort FileIdCount;
        public uint[] Offsets;

        public FATO(BinaryReader br)
        {
            Signature = br.ReadChars(4);
            BlockSize = br.ReadUInt32();

            FileIdCount = br.ReadUInt16();
            _ = br.ReadUInt16(); // padding

            Offsets = new uint[FileIdCount];
            for (int i = 0; i < FileIdCount; i++)
            {
                Offsets[i] = br.ReadUInt32();
            }
        }
    }

    public class FATB // File Allocation TaBle
    {
        public char[] Signature;
        public uint BlockSize;

        public int FileCount;
        public FATB_FileInfo[] Files;

        public FATB(BinaryReader br)
        {
            Signature = br.ReadChars(4);
            BlockSize = br.ReadUInt32();

            FileCount = br.ReadInt32();

            Files = new FATB_FileInfo[FileCount];
            for (int i = 0; i < FileCount; i++)
            {
                var fileinfo = new FATB_FileInfo();
                fileinfo.LanguageFlags = br.ReadUInt32();
                fileinfo.Variations = new FATB_VariationInfo[32];

                for (int l = 0; l < 32; l++)
                {
                    if (((fileinfo.LanguageFlags >> l) & 1) == 1)
                    {
                        var variation = new FATB_VariationInfo()
                        {
                            Begin = br.ReadInt32(),
                            End = br.ReadInt32(),
                            Length = br.ReadInt32(),
                        };
                        fileinfo.Variations[l] = variation;
                    }
                }

                Files[i] = fileinfo;
            }
        }
    }

    public class FATB_FileInfo
    {
        public uint LanguageFlags;
        public FATB_VariationInfo[] Variations;
    }

    public class FATB_VariationInfo
    {
        public int Begin;
        public int End;
        public int Length;
    }

    public class FIMG // File IMaGe
    {
        public char[] Signature;
        public uint BlockSize;

        public uint ImageSize;

        public FIMG(BinaryReader br)
        {
            Signature = br.ReadChars(4);
            BlockSize = br.ReadUInt32();

            ImageSize = br.ReadUInt32();
        }
    }

}
