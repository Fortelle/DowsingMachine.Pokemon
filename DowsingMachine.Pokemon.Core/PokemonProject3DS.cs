using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.Utilities;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProject3DS : FolderProject, IPokemonProject, IPreviewString
{
    [Option]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }

    protected Dictionary<string, string[]> LanguageMaps = new();

    protected Dictionary<string, string> MessageMaps = new();


    protected PokemonProject3DS() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);
    }

    protected class GarcReader : DataReaderBase<IEnumerable<byte[]>>, IDataReader<string, IEnumerable<byte[]>>
    {
        public IEnumerable<byte[]> Read(string filepath)
        {
            var garc = new GARC();
            garc.Open(filepath);
            return garc.AsEnumerable().Select(x => x.Data);
        }
    }

    public override void BeginWork()
    {
        base.BeginWork();
        MsgFormatter = Game.Title switch
        {
            GameTitle.Sun or GameTitle.Moon => new SunMoonMsgFormatter(),
            GameTitle.UltraSun or GameTitle.UltraMoon => new UltraSunUltraMoonMsgFormatter(),
            _ => new PokemonMsgFormatterV2(),
        };
    }

    private MsgFormatter MsgFormatter;

    private MsgWrapper GetWrapper(string filename, string langcode = null)
    {
        if (!MessageMaps.TryGetValue(filename, out var filename2)) return null;
        var v1 = filename2.Split("\\");
        var group = v1[0];
        var fileindex = int.Parse(v1[1]);
        if (langcode == null)
        {
            langcode = DowsingMachineApp.Config.Get<string>("PreviewLanguage");
            langcode = StringUtil.GetMostMatchedLangcode(LanguageMaps.Keys.ToArray(), langcode);
        }
        if (string.IsNullOrEmpty(langcode)) langcode = "ja-Jpan";
        var i = group == "message" ? 0 : 1;
        var relpath = LanguageMaps[langcode][i];
        var name = $"{group}\\{fileindex}";
        var wrapper = GetOrCreateCache(langcode + "_" + name, () => {
            var msgs = GetData<Lazy<MsgDataV2>[]>("msg", new GetDataOptions()
            {
                ReferenceArguments = new[] { relpath },
            });
            var wr = new MsgWrapper(msgs[fileindex].Value, name, FileVersion.GenVI, new[] { langcode });
            wr.Load();
            return wr;
        });
        return wrapper;
    }

    public string GetString(string filename, int value)
    {
        var wrapper = GetWrapper(filename);
        if (wrapper == null) return "";
        var entry = wrapper.TryGetEntry(value);
        if (entry == null || !entry.HasText) return "";
        var options = new StringOptions(StringFormat.Plain, "");
        return MsgFormatter.Format(entry[0], options);
    }

    public string[] GetStrings(string filename)
    {
        var wrapper = GetWrapper(filename);
        if (wrapper == null) return null;
        var options = new StringOptions(StringFormat.Plain, "");
        return wrapper.GetTextEntries().Select(x => MsgFormatter.Format(x[0], options)).ToArray();
    }

    public string GetPreviewString(params object[] args)
    {
        return GetString((string)args[0], int.Parse(args[1].ToString()));
    }

    [Data]
    public MultilingualCollection DumpMessage()
    {
        var mc = new MultilingualCollection
        {
            Version = FileVersion.GenVI,
            Formatter = MsgFormatter
        };

        var groupnames = new[] { "message", "script" };
        foreach (var (langcode, relpaths) in LanguageMaps)
        {
            var list = new List<MsgWrapper>();
            for (int i = 0; i < relpaths.Length; i++)
            {
                var msgs = GetData<Lazy<MsgDataV2>[]>("msg", new GetDataOptions()
                {
                    ReferenceArguments = new[] { relpaths[i] },
                    UseCache = false,
                });
                for (var j = 0; j < msgs.Length; j++)
                {
                    var k = j;
                    var wrp = new MsgWrapper($"{groupnames[i]}\\{j}", FileVersion.GenVI, new[] { langcode })
                    {
                        LazyLoad = (wr) => wr.Load(msgs[k].Value, null),
                    };
                    list.Add(wrp);
                }
            }
            mc.Wrappers.Add(langcode, list.ToArray());
        }
        return mc;
    }


}
