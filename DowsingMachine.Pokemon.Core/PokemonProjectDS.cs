using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Pokemon.Core.Gen4;
using PBT.DowsingMachine.Projects;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProjectDS : ParallelProject, IPokemonProject
{
    public GameInfo Game { get; set; }
    public Dictionary<string, string[]> LanguageMap { get; }
    public string[] LanguageCodes => LanguageMap[Variation];

    protected PokemonProjectDS(GameTitle title, string baseFolder, string lang)
        : base($"{title}", baseFolder, lang)
    {
        ((IPokemonProject)this).Set(title);
    }

    protected PokemonProjectDS(GameTitle title, string baseFolder, Dictionary<string, string[]> langs)
        : base($"{title}", baseFolder, langs.Keys.ToArray())
    {
        ((IPokemonProject)this).Set(title);

        LanguageMap = langs;
    }

    protected class NarcReader : DataReader<byte[][]>
    {
        public NarcReader(string path) : base(path)
        {
        }

        protected override byte[][] Open()
        {
            var path = Project.GetPath(RelatedPath);
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
            var mw = new MsgWrapper(msg, fn, LanguageCodes.First())
            {
                Group = Variation,
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

        mc.Wrappers.Add(Variation, wrappers);
        return mc;
    }

    [Dump]
    public IEnumerable<string> DumpMsgFile()
    {
        var oldfolder = Variation;
        foreach (var foldername in LanguageMap.Keys)
        {
            Switch(foldername);

            var outputFolder = Path.Combine(OutputPath, "msg", foldername);
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
        Switch(oldfolder);
    }

}
