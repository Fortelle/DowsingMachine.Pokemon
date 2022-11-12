using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Utilities;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProjectNS : ExtendableProject, IPokemonProject, IPreviewString
{
    public GameInfo Game { get; set; }

    private MultilingualCollection message;
    public MultilingualCollection Message
    {
        get
        {
            if (message == null)
            {
                message = GetData<MultilingualCollection>($"message");
            }
            return message;
        }
    }

    protected PokemonProjectNS(GameTitle title, string version, string baseFolder, string? patchFolder = null)
        : base($"{title}", version, baseFolder, patchFolder)
    {
        ((IPokemonProject)this).Set(title);

    }

    [Extraction]
    public IEnumerable<string> ExtractFiles(string output)
    {
        if (true)
        {
            var files = DirectoryUtil.GetFiles(Root, "*.gfpak"); //  + @"\romfs\bin\archive\chara\data\tr"
            foreach (var file in files)
            {
                using var gfpak = new GFPAK(file.Path);
                var outfolder = Path.Combine(
                    output,
                    "gfpak",
                    file.RelativeDirectoryName,
                    file.FileNameWithoutExtension
                    );

                foreach (var entry in gfpak.Entries)
                {
                    var path = Path.Combine(outfolder, entry.Parents[0], entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, entry.Data);

                    if (false && BinaryUtil.CheckSignature(entry.Data, "BNTX"))
                    {
                        using var bntx = new BNTX(entry.Data);
                        if (bntx.Name == null) continue;
                        var bntnPath = Path.Combine(
                            output,
                            "bntx_from_gfpak",
                            file.RelativeDirectoryName,
                            file.FileNameWithoutExtension,
                            Path.ChangeExtension(bntx.Name, ".bntx")
                            );
                        Directory.CreateDirectory(Path.GetDirectoryName(bntnPath));
                        File.WriteAllBytes(bntnPath, entry.Data);

                        var imagePath = Path.Combine(
                            output,
                            "png_from_gfpak_bntx",
                            file.RelativeDirectoryName,
                            file.FileNameWithoutExtension
                            );
                        bntx.Output(imagePath, BNTX.OutputMode.CreateFolderWhenMultiple);
                    }
                }

                yield return outfolder;
            }
        }

        if (false)
        {
            var files = DirectoryUtil.GetFiles(Root, "*.arc");
            foreach (var arcPath in files)
            {
                //var relpath = file.Replace(Root, "");
                using var sarc = new SARC(arcPath.Path);
                var outfolder = Path.Combine(
                    output,
                    "arc",
                    arcPath.RelativeDirectoryName,
                    arcPath.FileNameWithoutExtension
                    );

                foreach (var entry in sarc.Entries)
                {
                    if (entry.Name == "timg/__Combined.bntx")
                    {
                        var bntxPath = Path.Combine(
                            output,
                            arcPath.RelativeDirectoryName.Replace("romfs", "bntx_from_arc"),
                            arcPath.FileNameWithoutExtension + ".bntx"
                            );
                        Directory.CreateDirectory(Path.GetDirectoryName(bntxPath));
                        File.WriteAllBytes(bntxPath, entry.Data);
                        yield return bntxPath;

                        using var bntx = new BNTX(entry.Data);
                        if (bntx.Name == null) continue;
                        var imagePath = Path.Combine(
                            output,
                            arcPath.RelativeDirectoryName.Replace("romfs", "png_from_arc_bntx"),
                            arcPath.FileNameWithoutExtension
                            );
                        bntx.Output(imagePath, BNTX.OutputMode.IgnoreFilename);
                        yield return imagePath;
                    }
                    else if (entry.Name.Contains(".bntx"))
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }

        if (false)
        {
            var files = DirectoryUtil.GetFiles(Root, "*.bntx");
            foreach (var bntxFileInfo in files)
            {
                using var bntx = new BNTX(bntxFileInfo.Path);
                var outfolder = Path.Combine(
                    output,
                    bntxFileInfo.RelativeDirectoryName.Replace("romfs", "png_from_rom")
                    );

                bntx.Output(outfolder, BNTX.OutputMode.CreateFolderWhenMultiple);
                yield return outfolder;
            }
        }
    }

    public string GetPreviewString(object[] args)
    {
        var name = (string)args[0];
        switch (args[1])
        {
            //case EnumHash eh:
            //    {
            //        if (eh.IsEmpty) return "";
            //        return Message.GetString("zh-Hans", name, eh.Value);
            //    }
            case string s:
                {
                    return Message.GetString("zh-Hans", name, s);
                }
            case ulong l:
                {
                    return Message.GetString("zh-Hans", name, l);
                }
            default:
                {
                    var index = int.Parse(args[1].ToString());
                    return Message.GetString("zh-Hans", name, index);
                }
        }
    }

    protected class ItemReader : DataReader<byte[][]>
    {
        public int Offset { get; }
        public int Size { get; }

        public ItemReader(string path, int offset, int size) : base(path)
        {
            Offset = offset;
            Size = size;
        }

        protected override byte[][] Open()
        {
            var path = Project.GetPath(RelatedPath);
            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs);
            var num1 = br.ReadUInt16();
            var num2 = br.ReadUInt16();
            var num3 = br.ReadUInt16();
            br.BaseStream.Seek(Offset, SeekOrigin.Begin);
            var offset = br.ReadUInt32();
            var indexes = Enumerable.Range(0, num1)
                .Select(i => br.ReadUInt16())
                .ToArray();
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            var data = Enumerable.Range(0, num3)
                .Select(i => br.ReadBytes(Size))
                .ToArray();
            var data2 = indexes.Select(i => data[i]).ToArray();
            return data2;
        }
    }

    protected class MessageReader : DataReader<MultilingualCollection>
    {
        public Dictionary<string, string[]> LanguageMaps { get; set; }

        public MessageReader(string path, Dictionary<string, string[]> langmap) : base(path)
        {
            LanguageMaps = langmap;
        }

        protected override MultilingualCollection Open()
        {
            var path = Project.GetPath(RelatedPath);
            var mc = new MultilingualCollection
            {
                Formatter = Project switch
                {
                    //PokemonProjectArceus => new ArceusMsgFormatter(),
                    _ => new PokemonMsgFormatterV2(),
                }
            };

            foreach (var (foldername, langcodes) in LanguageMaps)
            {
                var langpath = Path.Combine(path, foldername) + "\\";
                var files = Directory.GetFiles(langpath, "*.dat", SearchOption.AllDirectories);
                var wrappers = files.Select(x => new MsgWrapper(x, FileVersion.GenVIII)
                {
                    Group = foldername,
                    LanguageCodes = langcodes,
                    Name = x.Replace(langpath, "").Replace(".dat", "")
                }).ToArray();
                mc.Wrappers.Add(langcodes[0], wrappers);
            }

            return mc;
        }
    }
}
