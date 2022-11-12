using GFMSG;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using PBT.DowsingMachine.Pokemon.Games;
using PBT.DowsingMachine.Projects;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PBT.DowsingMachine.Pokemon.Core.Gen9;

public class PokemonProjectSV : PokemonProjectNS
{
    public static readonly Dictionary<string, string[]> LanguageMaps = new()
    {
        ["JPN"] = new[] { "ja-Hrkt" },
        ["JPN_KANJI"] = new[] { "ja-Jpan" },
        ["English"] = new[] { "en-US" },
        ["French"] = new[] { "fr" },
        ["Italian"] = new[] { "it" },
        ["German"] = new[] { "de" },
        ["Spanish"] = new[] { "es" },
        ["Korean"] = new[] { "ko" },
        ["Simp_Chinese"] = new[] { "zh-Hans" },
        ["Trad_Chinese"] = new[] { "zh-Hant" },
    };

    public PokemonProjectSV(GameTitle title, string version, string baseFolder, string? patchFolder = null)
        : base(title, version, baseFolder, patchFolder)
    {
        AddReference("trpfd",
            new ByteReader(@"romfs\arc\data.trpfd"),
            FlatBufferConverter.DeserializeFrom<TRPFD>
            );

        AddReference("trpfs",
            new ArchiveReader<TRPFS>(@"romfs\arc\data.trpfs")
            );

        AddReference("personal",
            new ByteReader(@"trpak\avalondataai_common_raid01.bin.trpak\68AB38E2CF1281ED"),
            Metatable<PersonalSV>.Deserialize
            );

        AddReference($"message", new MessageReaderSV(@$"trpfs", LanguageMaps));
        AddReference($"messagereference", new DataInfo(@"trpak\messagedatSimp_Chinesecommon{0}.trpak"));

        AddReference("test", new DummyReader(), _ =>
        {
            return new[]
            {
                new AAA(),
                new AAA(),
                new AAA(),
            };
        });
    }

    public class AAA
    {
        [StringReference(@"monsname2")] public int A { get; set; }
        [StringReference(@"monsname")] public int B { get; set; }
        [StringReference(@"monsname")] public int C { get; set; }
    }

    protected override MsgWrapper GetPreviewMsgWrapper(object[] args)
    {
        var name = (string)args[0];
        var info = GetData<DataInfo>("messagereference");
        var path1 = info.RelatedPath.Replace("{0}", name + ".tbl");
        var path2 = info.RelatedPath.Replace("{0}", name + ".dat");
        var files = new List<string>();
        var pf1 = GetPairedFiles(path1, "*");
        var pf2 = GetPairedFiles(path2, "*");
        var pfs = pf1.Concat(pf2).ToArray();
        if (pfs.Length == 0)
        {
            return null;
        }
        else
        {
            var data2 = pfs.Select(x => File.ReadAllBytes(x.Newer)).ToArray();

            if (new string(Encoding.Default.GetChars(data2[0], 0, 4)) == "AHTB")
            {
                data2 = data2.Reverse().ToArray();
            }

            var msg = new MsgDataV2(data2[0]);
            var ahtb = new AHTB(data2[1]);
            var wrapper = new MsgWrapper();
            wrapper.Load(msg, ahtb);
            return wrapper;
        }
    }

    [Extraction]
    public IEnumerable<string> ExtractTrpfs()
    {
        var trpfd = GetData<TRPFD>($"trpfd");
        var trpfs = GetData<TRPFS>($"trpfs");

        var outputFolder = Path.Combine(PatchFolder ?? Root, "trpfs");

        for (var i = 0; i < trpfd.Filenames.Length; i++)
        {
            var hash = Utilities.Codecs.FnvHash.Fnv1a_64(trpfd.Filenames[i]);
            var data = trpfs[hash];
            var filename = trpfd.Filenames[i].Replace("arc/", "");
            var filepath = Path.Combine(outputFolder, filename);
            File.WriteAllBytes(filepath, data);
            yield return filepath;
        }
    }

    [Extraction]
    public IEnumerable<string> ExtractTrpak()
    {
        var originFolder = Path.Combine(Root, "trpak");
        var patchFolder = Path.Combine(PatchFolder ?? Root, "trpak"); 

        var trpfd = GetData<TRPFD>($"trpfd");
        var trpfs = GetData<TRPFS>($"trpfs");

        var md5Algo = MD5.Create();
        var hasPatch = originFolder != patchFolder;

        for (var i = 0; i < trpfd.Filenames.Length; i++)
        {
            var hash = Utilities.Codecs.FnvHash.Fnv1a_64(trpfd.Filenames[i]);
            var data = trpfs[hash];

            var foldername = trpfd.Filenames[i].Replace("arc/", "");
            var folderexists = Directory.Exists(Path.Combine(patchFolder, foldername));

            var trpak = new Trpak();
            trpak.Open(data);
            for (var j = 0; j < trpak.Entries.Length; j++)
            {
                var filename = trpak.Hashes[j].ToString("X16");
                var filedata = trpak[j];
                var originFilepath = Path.Combine(originFolder, foldername, filename);
                var patchFilepath = Path.Combine(patchFolder, foldername, filename);

                if (hasPatch && File.Exists(originFilepath))
                {
                    using var ms = File.OpenRead(originFilepath);
                    var originMd5 = BitConverter.ToString(md5Algo.ComputeHash(ms));
                    var patchMd5 = BitConverter.ToString(md5Algo.ComputeHash(filedata));
                    if (originMd5 == patchMd5) continue;
                }
                if (!folderexists)
                {
                    Directory.CreateDirectory(Path.Combine(patchFolder, foldername));
                    folderexists = true;
                }
                File.WriteAllBytes(patchFilepath, filedata);
            }
            yield return foldername;
        }
    }

    [Test]
    public void aaa()
    {
        var folder = @"E:\Pokemon\Resources\Unpacked\NS\SV\bntx";
        var dirs = Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly);
        foreach(var dir in dirs)
        {
            var dirs2 = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            foreach(var dir2 in dirs2)
            {
                var files = Directory.GetFiles(dir2, "*", SearchOption.AllDirectories);
                if(files.Length == 1)
                {
                    var newfile = Regex.Replace(files[0], @"\\[A-F0-9]{16}\\", "\\");
                    if (!File.Exists(newfile))
                    {
                        File.Move(files[0], newfile);
                        Directory.Delete(dir2);
                    }
                }
            }
        }
    }

    [Test]
    public void Resize()
    {
        var fbs = new FolderBrowserDialog();
        if (fbs.ShowDialog() != DialogResult.OK) return;

        var inputFolder = fbs.SelectedPath;
        var outputFolder = Path.Combine(Path.GetDirectoryName(inputFolder), Path.GetFileName(inputFolder) + "_resized");
        Directory.CreateDirectory(outputFolder);

        var files = Directory.GetFiles(inputFolder, "*.png");
        foreach(var file in files)
        {
            using var bmp = Bitmap.FromFile(file);
            using var bmp2 = new Bitmap(bmp, new Size(1024, 536));
            var outputPath = Path.Combine(outputFolder, Path.GetFileName(file));
            bmp2.Save(outputPath);
        }

    }

    [Test]
    public string[] Find()
    {
        var files = GetFiles(@"\trpak\", "*", PatchReadMode.OnlyPatch);
        var bf = new Utilities.BinaryFinder.BmhFinder(new byte[] {
            155, 110, 125, 55, 80, 45
        });
        var sb = new List<string>();

        foreach (var file in files)
        {
            if (file.RelativePath.Contains(@"\messagedat"))
            {
                continue;
            }

            using var ms = File.OpenRead(file.Path);
            var signature = new[]
            {
                ms.ReadByte(),
                ms.ReadByte(),
                ms.ReadByte(),
                ms.ReadByte(),
            };
            if (signature[0] > 0 && (signature[0] & 0xb11) == 0 && signature[1] == 0 && signature[2] == 0 && signature[3] == 0)
            {
                var buffer = new byte[ms.Length];
                ms.Read(buffer, 0, (int)ms.Length);

                foreach (var result in bf.Find(buffer))
                {
                    sb.Add($"{file.Path}, 0x{result:X8}");
                }

            }
        }

        return sb.ToArray();
    }


    [Extraction]
    public IEnumerable<string> ExtractSarc()
    {
        var files = GetFiles(@"\trpak\", "*", PatchReadMode.OnlyPatch);
        //bool skip = true;
        int c = 0;
        foreach (var file in files)
        {
            //if (file.RelativePath == "envmastermasterirradiancevolume.bntx.trpak\\21EFBE760D6D9DCB")
            //{
            //    continue;
            //}
            //if (file.RelativePath == "system_resourcescene_assetspick_iconmodelpick_icon.trmdl.trpak\\58C3C78C6A45A00A")
            //{
            //    skip = false;
            //    continue;
            //}
            //if (file.RelativePath == "pokemondatapm0603pm0603_00_00pm0603_00_00_base.trslp.trpak\\1C4A0FC2B447C0CD")
            //{
            //    skip = false;
            //    continue;
            //}
            //if (skip) continue;
            if (file.RelativePath.Contains(@"\messagedat"))
            {
                continue;
            }

            using var ms = File.OpenRead(file.Path);
            var signature = $"{(char)ms.ReadByte()}{(char)ms.ReadByte()}{(char)ms.ReadByte()}{(char)ms.ReadByte()}";

            if (signature == "SARC")
            {
                ms.Position = 0;
                using var sarc = new SARC();
                sarc.Open(ms);

                var bntxentry = sarc.Entries.FirstOrDefault(x => x.Name == "timg/__Combined.bntx");
                if (bntxentry != null)
                {
                    var outfolder = file.Path.Replace(@"\trpak", @"\sarc_bntx");
                    using var bntx = new BNTX(bntxentry.Data);
                    if (bntx.Name == null) continue;
                    bntx.Output(outfolder, BNTX.OutputMode.IgnoreFilename);
                }
                else
                {
                    var outfolder = file.Path.Replace(@"\trpak", @"\sarc");
                    foreach (var entry in sarc.Entries)
                    {
                        var filepath = Path.Combine(outfolder, entry.Name);
                        Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                        File.WriteAllBytes(filepath, entry.Data);
                    }
                }
            }
            else if (signature == "BNTX")
            {
                ms.Position = 0;
                var outfolder = file.Path.Replace(@"\trpak", @"\bntx");
                using var bntx = new BNTX(ms);
                if (bntx.Name == null) continue;
                outfolder = Path.GetDirectoryName(outfolder); 
                bntx.Output(outfolder, BNTX.OutputMode.IgnoreFilename);
                bntx.Dispose();
            }

            if (c++ >= 1000)
            {
                GC.Collect();
                c = 0;
            }
            yield return file.Path;
        }
    }

    [Dump]
    public IEnumerable<string> DumpLearnsets()
    {
        var outputFolder = Path.Combine(OutputPath, "learnset");
        Directory.CreateDirectory(outputFolder);
        var personals = GetData<PersonalSV[]>("personal");
        var prefix = $"scarletviolet_{Version.Major}.{Version.Minor}.{Version.Build}";
        var format = "{0:0000}.{1:00}";

        {
            var lt = new LearnsetTable();
            foreach(var pm in personals)
            {
                var id = new PokemonId(pm.NumForm.Number, pm.NumForm.Form);
                var data = pm.Waza_level.Select(x => $"{x.Waza}:{x.Level}").ToArray();
                lt.Add(id, data);
            }
            var path = Path.Combine(outputFolder, $"{prefix}.levelup.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            foreach (var pm in personals)
            {
                var id = new PokemonId(pm.NumForm.Number, pm.NumForm.Form);
                var data = pm.Waza_machine.Select(x => $"{x}").ToArray();
                lt.Add(id, data);
            }
            var path = Path.Combine(outputFolder, $"{prefix}.tm.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            foreach (var pm in personals)
            {
                var id = new PokemonId(pm.NumForm.Number, pm.NumForm.Form);
                var data = pm.Waza_egg.Select(x => $"{x}").ToArray();
                lt.Add(id, data);
            }
            var path = Path.Combine(outputFolder, $"{prefix}.egg.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            foreach (var pm in personals)
            {
                var id = new PokemonId(pm.NumForm.Number, pm.NumForm.Form);
                var data = pm.Waza_tutor.Select(x => $"{x}").ToArray();
                lt.Add(id, data);
            }
            var path = Path.Combine(outputFolder, $"{prefix}.tutor.txt");
            lt.Save(path, format);
            yield return path;
        }


    }


}
