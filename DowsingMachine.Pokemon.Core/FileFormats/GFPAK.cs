using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Utilities.Codecs;
using SoulsFormats;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

// GraPHic PAcKage
public class GFPAK : ICollectionArchive<byte[]>, ILargeArchive
{
    public const string Magic = "GFLXPACK"; // 0x4B434150_584C4647

    public GfpakHeader Header { get; set; }
    public ulong[] FileHashArray { get; set; }
    public FolderInfo[] Folders { get; set; }
    public FileDataInfo[] Files { get; set; }

    public byte[] this[int index] => GetData(index);
    public byte[] this[uint hash] => GetData(Array.IndexOf(FileHashArray, hash));
    public byte[] this[string name] => GetData(Array.IndexOf(FileHashArray, FnvHash.Fnv1a_64(name)));

    private Stream Stream { get; set; }
    private BinaryReader Reader { get; set; }


    public GFPAK()
    {
    }

    public GFPAK(string path) => Open(path);

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
            foreach (var folder in Folders)
            {
                var parents = new string[] { folder.Hash.ToString("X8") };
                foreach (var file in folder.FileInfoArray)
                {
                    var data = GetData(file.FileIndex);
                    var name = file.Hash.ToString("X8");
                    yield return new Entry<byte[]>(data, name, file.FileIndex)
                    {
                        Parents = parents.ToArray(),
                    };
                }
            }
        }
    }

    private byte[] GetData(int index)
    {
        if (index == -1) return null;
        var info = Files[index];
        Reader.BaseStream.Position = (long)info.OffsetPacked;
        var data = Reader.ReadBytes((int)info.CompressedSize);
        if (info.CompressionType > 0)
        {
            data = Decompress(data, (int)info.DecompressedSize, info.CompressionType);

        }
        return data;
    }

    private void Load(BinaryReader br)
    {
        Header = new GfpakHeader(br);

        Debug.Assert(br.BaseStream.Position == Header.HashArrayOffset);
        FileHashArray = Enumerable.Range(0, Header.FileCount)
            .Select(x => br.ReadUInt64())
            .ToArray();

        Debug.Assert(br.BaseStream.Position == Header.FolderOffset[0]);
        Folders = Enumerable.Range(0, Header.FolderCount)
            .Select(i =>
            {
                var folderHash = br.ReadUInt64();
                var fileCount = br.ReadInt32();
                br.ReadUInt32(); // padding 0xCCCC
                var fileInfos = Enumerable.Range(0, fileCount)
                    .Select(_ =>
                    {
                        var fileHash = br.ReadUInt64();
                        var fileIndex = br.ReadInt32();
                        br.ReadUInt32(); // padding 0xCCCC
                        return new FileHashInfo()
                        {
                            Hash = fileHash,
                            FileIndex = fileIndex,
                        };
                    }).ToArray();
                return new FolderInfo()
                {
                    Hash = folderHash,
                    FileCount = fileCount,
                    FileInfoArray = fileInfos,
                };
            }).ToArray();

        Debug.Assert(br.BaseStream.Position == Header.FileInfoOffset);
        Files = Enumerable.Range(0, Header.FileCount)
            .Select(i =>
            {
                var level = br.ReadUInt16();
                var type = br.ReadUInt16();
                var decSize = br.ReadUInt32();
                var comSize = br.ReadUInt32();
                br.ReadInt32(); // padding 0xCCCC
                var offset = br.ReadUInt64();
                return new FileDataInfo(level, type, decSize, comSize, offset);
            }).ToArray();
    }

    private static byte[] Decompress(byte[] compressedData, int decompressedLength, ushort type)
    {
        switch (type)
        {
            case 0:
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

    public void Dispose()
    {
        Stream?.Dispose();
        Reader?.Dispose();
    }


    //[StructLayout(LayoutKind.Sequential)]
    public class GfpakHeader
    {
        public const int Size = 0x18;

        public string Signature;
        public uint Version;
        public uint IsRelocated;
        public int FileCount;
        public int FolderCount;

        public long FileInfoOffset;
        public long HashArrayOffset;
        public long[] FolderOffset;

        public GfpakHeader(BinaryReader br)
        {
            Signature = new string(br.ReadChars(8));
            Version = br.ReadUInt32();
            IsRelocated = br.ReadUInt32();
            FileCount = br.ReadInt32();
            FolderCount = br.ReadInt32();

            FileInfoOffset = br.ReadInt64();
            HashArrayOffset = br.ReadInt64();
            FolderOffset = Enumerable.Range(0, FolderCount).Select(x => br.ReadInt64()).ToArray();
        }
    }

    public class FolderInfo
    {
        public ulong Hash;
        public int FileCount;
        public FileHashInfo[] FileInfoArray;
    }

    public struct FileHashInfo
    {
        public ulong Hash;
        public int FileIndex;
    }

    public record struct FileDataInfo(
        ushort Level,
        ushort CompressionType,
        uint DecompressedSize,
        uint CompressedSize,
        ulong OffsetPacked
    );
}
