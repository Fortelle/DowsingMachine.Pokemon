using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Pokemon.Core.Gen4;
using PBT.DowsingMachine.Projects;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProjectDS : FolderProject, IPokemonProject
{
    [Option]
    public string LanguageCode { get; set; }

    [Option]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);

    }

    protected class NarcReader : DataReader<byte[][]>
    {
        public NarcReader(string path) : base(path)
        {
        }

        protected override byte[][] Open()
        {
            var path = Project.As<IFolderProject>().GetPath(RelatedPath);
            var narc = new NARC();
            narc.Open(path);
            return narc.Entries.Select(x => x.Data).ToArray();
        }
    }

    public MultilingualCollection ReadMessage(byte[][] narcData)
    {
        var mc = new MultilingualCollection
        {
            Version = FileVersion.GenIV,
            Formatter = new DpMsgFormatter(),
        };

        var wrappers = narcData.Select((data, i) =>
        {
            var msg = new MsgDataV1(data);
            var fn = msg.Seed.ToString();
            var mw = new MsgWrapper(msg, fn, LanguageCode)
            {
                Group = LanguageCode,
            };
            return mw;
        }).ToArray();

        var hashes = DpRes.MsgFilenames
            .Select(fn => MsgDataV1.CalcCrc($@"./data/{fn}.dat"))
            .ToArray();
        if (Game.Title is GameTitle.Diamond or GameTitle.Pearl or GameTitle.Platinum)
        {
            var j = 0;
            for (var i = 0; i < wrappers.Length; i++)
            {
                var hash = int.Parse(wrappers[i].Name!);
                while (true)
                {
                    var hash2 = hashes[j];
                    if (hash == hash2)
                    {
                        wrappers[i].Name = DpRes.MsgFilenames[j];
                        break;
                    }
                    j++;
                }
            }
        }
        else
        {
            for (var i = 0; i < wrappers.Length; i++)
            {
                var hash = ushort.Parse(wrappers[i].Name!);
                var x = Array.IndexOf(hashes, hash);
                if (x > -1)
                {
                    wrappers[i].Name = DpRes.MsgFilenames[x];
                }
            }
        }

        mc.Wrappers.Add(LanguageCode, wrappers);
        return mc;
    }

    [Dump]
    public IEnumerable<string> DumpMsgFile()
    {
        var outputFolder = Path.Combine(OutputFolder, "msg", LanguageCode);
        Directory.CreateDirectory(outputFolder);

        var narc = GetData<byte[][]>("msg", 0);

        var hashes = DpRes.MsgFilenames
            .Select(fn => MsgDataV1.CalcCrc($@"./data/{fn}.dat"))
            .ToArray();
        if (Game.Title is GameTitle.Diamond or GameTitle.Pearl or GameTitle.Platinum)
        {
            var j = 0;
            foreach (var data in narc)
            {
                var msg = new MsgDataV1(data);
                var name = "";
                while (true)
                {
                    if (msg.Seed == hashes[j])
                    {
                        name = DpRes.MsgFilenames[j];
                        break;
                    }
                    j++;
                }
                Debug.Assert(name != null);
                var path = Path.Combine(outputFolder, name + ".dat");
                File.WriteAllBytes(path, data);
                yield return path;
            }
        }
        else
        {
            var j = 0;
            for (var i = 0; i < narc.Length; i++)
            {
                var path = Path.Combine(outputFolder, i.ToString() + ".dat");
                File.WriteAllBytes(path, narc[i]);
                yield return path;
            }
        }
    }

}
