using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;
using System.Text;
using System.Text.RegularExpressions;

namespace PBT.DowsingMachine.Pokemon.Core.Gen9;

public class MessageReaderSV : DataReader<MultilingualCollection>
{
    public Dictionary<string, string[]> LanguageMaps { get; set; }

    public MessageReaderSV(string path, Dictionary<string, string[]> langmap) : base(path)
    {
        LanguageMaps = langmap;
    }

    protected override MultilingualCollection Open()
    {
        var abspath = Project.As<PokemonProjectSV>().GetPath(RelatedPath);
        var filelist = Directory.GetFiles(abspath, "messagedat*.trpak");

        var mc = new MultilingualCollection
        {
            Formatter = new PokemonMsgFormatterV2()
        };

        foreach (var (langname, langcodes) in LanguageMaps)
        {
            var wrappers = new List<MsgWrapper>();
            foreach (var group in new[] { "script", "common" })
            {
                var files = filelist.Where(x => x.Contains($"messagedat{langname}{group}")).ToArray();

                foreach (var file in files)
                {
                    var trpak = Trpak.Load(file);
                    if (trpak.Entries.Length == 1) continue;
                    var data1 = trpak[0];
                    var data2 = trpak[1];
                    if (new string(Encoding.Default.GetChars(data1, 0, 4)) == "AHTB")
                    {
                        (data1, data2) = (data2, data1);
                    }
                    var msg = new MsgDataV2(data1);
                    var ahtb = new AHTB(data2);
                    var name = Regex.Replace(file, @"^.+?messagedat" + langname + group + @"(.+?)\..+$", group + "\\$1");
                    var wrapper = new MsgWrapper(msg, ahtb, name, FileVersion.GenVIII, langcodes)
                    {
                        Group = langname,
                    };
                    wrappers.Add(wrapper);
                }
            }
            mc.Wrappers.Add(langcodes[0], wrappers.ToArray());
        }

        return mc;
    }

}
