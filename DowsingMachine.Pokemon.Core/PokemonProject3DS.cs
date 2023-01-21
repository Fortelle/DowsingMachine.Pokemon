using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProject3DS : FolderProject, IPokemonProject, IPreviewString
{
    [Option]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }


    protected PokemonProject3DS() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);
    }

    protected class GarcReader : DataReader<byte[][]>
    {
        public GarcReader(string path) : base(path)
        {
        }

        protected override byte[][] Open()
        {
            var path = Project.As<IFolderProject>().GetPath(RelatedPath);
            var garc = new GARC();
            garc.Open(path);
            return garc.Entries.Select(x => x.Data).ToArray();
        }
    }

    public string GetPreviewString(params object[] args)
    {
        var name = (string)args[0];
        var value = int.Parse(args[1].ToString());
        var mc = GetData<MultilingualCollection>("messagereference", CacheMode.CacheFinal);
        return mc.GetString("zh-hans", name, value);
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
            var mc = new MultilingualCollection
            {
                Version = FileVersion.GenVI,
                Formatter = ((PokemonProject3DS)Project).Game.Title switch
                {
                    GameTitle.Sun or GameTitle.Moon => new SunMoonMsgFormatter(),
                    GameTitle.UltraSun or GameTitle.UltraMoon => new UltraSunUltraMoonMsgFormatter(),
                    _ => new PokemonMsgFormatterV2(),
                }
            };

            var groupnames = new[] { "message", "script_message" };
            foreach (var (langcode, relpaths) in LanguageMaps)
            {
                var list = new List<MsgWrapper>();
                for (int i = 0; i < relpaths.Length; i++)
                {
                    using var garc = new GARC();
                    var path = Project.As<IFolderProject>().GetPath(relpaths[i]);
                    garc.Open(path);
                    foreach (var entry in garc.Entries)
                    {
                        var msg = new MsgDataV2(entry.Data);
                        var wrp = new MsgWrapper(msg, $"{groupnames[i]}\\{entry.Index}", FileVersion.GenVI, new[] { langcode });
                        list.Add(wrp);
                    }
                }
                mc.Wrappers.Add(langcode, list.ToArray());
            }
            return mc;
        }
    }

}
