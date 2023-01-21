using PBT.DowsingMachine.Data;

namespace PBT.DowsingMachine.Pokemon.Core.Colosseums;

public class DECK
{
    public char[] Magic; // DECK
    public int FileSize;
    public int FileCount;
    public List<DDPK> Entries = new();

    public DECK(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReaderEx(fs) { IsBigEndian = true };
        Load(br);
    }

    public DECK(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReaderEx(ms) { IsBigEndian = true };
        Load(br);
    }

    public byte[][] Read(string name)
    {
        return Entries.First(x => x.Name == name).Entries.ToArray();
    }

    private void Load(BinaryReaderEx br)
    {
        Magic = br.ReadChars(4);
        FileSize = br.ReadInt32();
        FileCount = br.ReadInt32();
        FileCount += 1;
        br.ReadInt32();

        for (var i = 0; i < FileCount; i++)
        {
            var entry = new DDPK(br);
            Entries.Add(entry);
        }
    }

    public class DDPK
    {
        public string Name;

        public char[] Magic; // DDPK
        public int FileSize;
        public int FileCount;
        public List<byte[]> Entries = new();

        public DDPK(BinaryReaderEx br)
        {
            Magic = br.ReadChars(4);
            FileSize = br.ReadInt32();
            FileCount = br.ReadInt32();
            if (FileCount == 0) FileCount = 1;
            br.ReadInt32();
            var length = (FileSize - 0x10) / FileCount;
            for (var i = 0; i < FileCount; i++)
            {
                Entries.Add(br.ReadBytes(length));
            }

            Name = new string(Magic);
        }
    }
}
