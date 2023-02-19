using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;
using System.ComponentModel;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProjectDS : FolderProject, IPokemonProject
{
    [Option]
    [TypeConverter(typeof(StringSelectConverter))]
    [Select("jpn", "eng", "fra", "ita", "ger", "spa", "kor")]
    public string Language { get; set; }

    [Option]
    public virtual GameTitle Title { get; set; }

    public GameInfo Game { get; set; }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);

    }

    protected class NarcReader : DataReaderBase<byte[][]>, IDataReader<string, byte[][]>
    {
        public byte[][] Read(string filepath)
        {
            var narc = new NARC();
            narc.Open(filepath);
            return narc.AsEnumerable().Select(x => x.Data).ToArray();
        }
    }

    protected class OverlayReader : DataReaderBase<BinaryReader>, IDataReader<string, BinaryReader>
    {
        public int Position { get; set; }

        public OverlayReader(int position)
        {
            Position = position;
        }

        public BinaryReader Read(string filepath)
        {
            var overlay = new Overlay(filepath);
            var ms = new MemoryStream(overlay.Data);
            var br = new BinaryReader(ms);
            if (Position > 0)
            {
                ms.Position = Position;
            }
            return br;
        }
    }
}
