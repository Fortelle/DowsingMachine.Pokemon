using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Structures;
using PBT.DowsingMachine.Utilities;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProjectNS : ExtendableProject, IPokemonProject
{
    [Option]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }

    protected PokemonProjectNS() : base()
    {
        Resources.Add(new DataResource("main")
        {
            Reference = new FileRef(@"exefs\main"),
            Reader = new FileReader<NSO0>(),
            Previewable = false,
        });
    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);
    }

    [Action]
    public IEnumerable<string> ExtractAllGfpak()
    {
        var files = DirectoryUtil.GetFiles(SourceFolder, "*.gfpak");
        foreach (var file in files)
        {
            using var gfpak = new GFPAK(file.FullPath);
            var outfolder = Path.Combine(OutputFolder, "gfpak", file.RelativePart);

            foreach (var entry in gfpak.AsEnumerable())
            {
                Directory.CreateDirectory(entry.GetDirectoryName());
                var path = Path.Combine(outfolder, entry.GetFullpath());
                File.WriteAllBytes(path, entry.Data);
            }

            yield return outfolder;
        }
    }

    [Action]
    public IEnumerable<string> ExtractAllPngInGfpak()
    {
        var files = DirectoryUtil.GetFiles(SourceFolder, "*.gfpak");
        foreach (var file in files)
        {
            using var gfpak = new GFPAK(file.FullPath);
            foreach (var entry in gfpak.AsEnumerable())
            {
                if (BinaryUtil.CheckSignature(entry.Data, "BNTX"))
                {
                    using var bntx = new BNTX(entry.Data);
                    if (bntx.Name == null) continue;
                    var outfolder = Path.Combine(OutputFolder, "gfpak_png", file.RelativePart);
                    bntx.Output(outfolder, BNTX.OutputMode.CreateFolderWhenMultiple);
                    yield return outfolder;
                }
            }
        }
    }

    [Action]
    public IEnumerable<string> ExtractAllBntx()
    {
        var files = DirectoryUtil.GetFiles(SourceFolder, "*.bntx");
        foreach (var file in files)
        {
            var outfolder = Path.Combine(OutputFolder, "bntx_png", file.RelativePart);
            using var bntx = new BNTX(file.FullPath);
            bntx.Output(outfolder, BNTX.OutputMode.CreateFolderWhenMultiple);
            yield return outfolder;
        }
    }

    [Action]
    public IEnumerable<string> ExtractAllArc()
    {
        var files = DirectoryUtil.GetFiles(SourceFolder, "*.arc");
        foreach (var arcPath in files)
        {
            using var sarc = new SARC(arcPath.FullPath);

            foreach (var entry in sarc.AsEnumerable())
            {
                var path = Path.Combine(OutputFolder, "arc", arcPath.RelativePart);
                Directory.CreateDirectory(path);
                path = Path.Combine(path, entry.Name);
                File.WriteAllBytes(path, entry.Data);
            }
            yield return arcPath.FullPath;
        }
    }

    [Action]
    public IEnumerable<string> ExtractAllPngInArc()
    {
        var files = DirectoryUtil.GetFiles(SourceFolder, "*.arc");
        foreach (var arcPath in files)
        {
            using var sarc = new SARC(arcPath.FullPath);

            foreach (var entry in sarc.AsEnumerable())
            {
                if (entry.Name == "timg/__Combined.bntx")
                {
                    using var bntx = new BNTX(entry.Data);
                    if (bntx.Name == null) continue;
                    var path = Path.Combine(OutputFolder, "arc_png", arcPath.RelativePart);
                    bntx.Output(path, BNTX.OutputMode.IgnoreFilename);
                    yield return path;
                }
            }
        }
    }



    protected class MessageReader : DataReaderBase<MultilingualCollection>, IDataReader<PathInfo[], MultilingualCollection>
    {
        public Dictionary<string, string[]> LanguageMaps { get; set; }

        public MessageReader(Dictionary<string, string[]> langmap)
        {
            LanguageMaps = langmap;
        }

        public MultilingualCollection Read(PathInfo[] files)
        {
            var mc = new MultilingualCollection
            {
                Formatter = new PokemonMsgFormatterV2(),
            };

            var langgroups = files.GroupBy(x => x.RelativePart.Split('/', '\\')[0]);

            foreach (var langgroup in langgroups)
            {
                if (!LanguageMaps.TryGetValue(langgroup.Key, out var langcodes)) continue;
                var wrappers = langgroup
                    .Where(x => x.FullPath.EndsWith(".dat"))
                    .Select(x => new MsgWrapper(x.FullPath, FileVersion.GenVIII)
                    {
                        LanguageCodes = langcodes,
                        Name = x.RelativePart.Replace(langgroup.Key + '\\', "").Replace(".dat", "")
                    }).ToArray();
                mc.Wrappers.Add(langgroup.Key, wrappers);
            }

            return mc;
        }
    }

}