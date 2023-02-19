namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class GFB
{
    public GFB_Header Header;

    public GFB()
    {
        
    }

    public void Load(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        Load(br);
    }

    public void Load(BinaryReader br)
    {
        Header = new GFB_Header()
        {
            magic = br.ReadInt32(),
            a = br.ReadInt16(),
            b = br.ReadInt16(),
            BodySize = br.ReadInt32(),
            d = br.ReadInt32(),
            ItemCount = br.ReadInt32(),
        };

        var offsets = new uint[Header.ItemCount];
        for(var i = 0; i < Header.ItemCount; i++)
        {
            offsets[i] = br.ReadUInt32();
        }


    }

    public struct GFB_Header
    {
        public int magic;
        public int a;
        public int b;
        public int BodySize;
        public int d;
        public int ItemCount;
    }

    public struct GFB_Item
    {
        public int magic;
        public int a;
        public int b;
        public int BodySize;
        public int d;
        public int ItemCount;
    }
}
