using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class OverlayReader : StreamBinaryReader
{
    public OverlayReader(string path, int offset) : base(path, offset)
    {
    }

    protected override Cache Open()
    {
        var path = Project.As<IFolderProject>().GetPath(RelatedPath);
        var overlay = new Overlay(path);
        var ms = new MemoryStream(overlay.Data);
        var br = new BinaryReader(ms);
        return new Cache(ms, br);
    }
}