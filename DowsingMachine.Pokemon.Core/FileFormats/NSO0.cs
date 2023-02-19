using PBT.DowsingMachine.Data;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class NSO0 : IArchive
{
    public NSO0Header Header;
    public byte[] Text;
    public byte[] Rodata;
    public byte[] Data;

    public NSO0()
    {
        
    }

    public void Open(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        Load(br);
    }

    public void Open(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        Load(br);
    }

    private void Load(BinaryReader br)
    {
        Header = new NSO0Header(br);
        Debug.Assert(br.BaseStream.Position == 0x100);

        for (var i = 0; i < 3; i++)
        {
            var isCompressed = (Header.Flags >> i & 0b1) == 0b1;
            var checkHash = (Header.Flags >> (i + 3) & 0b1) == 0b1;
            var offset = i switch
            {
                0 => Header.TextHeader.FileOffset,
                1 => Header.RodataHeader.FileOffset,
                2 => Header.DataHeader.FileOffset,
            };
            var compLength = Header.CompressedSizes[i];
            var decompLength = i switch
            {
                0 => Header.TextHeader.DecompressedSize,
                1 => Header.RodataHeader.DecompressedSize,
                2 => Header.DataHeader.DecompressedSize,
            };
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            var data = br.ReadBytes(compLength);
            if (isCompressed)
            {
                var dest = new byte[decompLength];
                K4os.Compression.LZ4.LZ4Codec.Decode(data, dest);
                data = dest;
            }
            switch (i)
            {
                case 0: Text = data; break;
                case 1: Rodata = data; break;
                case 2: Data = data; break;
            }
        }
    }


    public record SegmentHeader(uint FileOffset, uint MemoryOffset, uint DecompressedSize);
    public record SegmentHeaderRelative(uint Offset, uint Size);

    public class NSO0Header
    {
        public char[] Magic; // Always NSO0
        public uint Version; // Always 0
        public uint Reserved;
        public uint Flags;
        public SegmentHeader TextHeader;
        public uint ModuleOffset;
        public SegmentHeader RodataHeader;
        public uint ModuleFileSize;
        public SegmentHeader DataHeader;
        public uint BssSize;

        public byte[] BuildId;
        public int[] CompressedSizes;

        public ulong dynstr_extents;
        public ulong dynsym_extents;
        public SegmentHeaderRelative[] SegmentHeaderRelatives;
        public byte[][] SectionHashes;

        public NSO0Header(BinaryReader br)
        {
            Magic = br.ReadChars(4);
            Version = br.ReadUInt32();
            Reserved = br.ReadUInt32();
            Flags = br.ReadUInt32();
            TextHeader = new SegmentHeader(br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32());
            ModuleOffset = br.ReadUInt32();
            RodataHeader = new SegmentHeader(br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32());
            ModuleFileSize = br.ReadUInt32();
            DataHeader = new SegmentHeader(br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32());
            BssSize = br.ReadUInt32();
            BuildId = br.ReadBytes(0x20);
            CompressedSizes = Enumerable.Range(0, 3).Select(i => (int)br.ReadUInt32()).ToArray();
            br.ReadBytes(0x1C);
            SegmentHeaderRelatives = Enumerable.Range(0, 3).Select(i => new SegmentHeaderRelative(br.ReadUInt32(), br.ReadUInt32())).ToArray();
            SectionHashes = Enumerable.Range(0, 3).Select(i => br.ReadBytes(0x20)).ToArray();
        }
    }


}
