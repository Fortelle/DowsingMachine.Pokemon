using GFMSG;
using GFMSG.GUI;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProject3DS : DataProject, IPokemonProject
{
    public GameInfo Game { get; set; }

    protected PokemonProject3DS(GameTitle title, string baseFolder)
        : base($"{title}", baseFolder)
    {
        ((IPokemonProject)this).Set(title);
    }

    protected class GarcReader : DataReader<byte[][]>
    {
        public GarcReader(string path) : base(path)
        {
        }

        protected override byte[][] Open()
        {
            var path = Project.GetPath(RelatedPath);
            var garc = new GARC();
            garc.Open(path);
            return garc.Entries.Select(x => x.Data).ToArray();
        }
    }


    [Test]
    public virtual void TestMessage()
    {
        var msg = GetData<MultilingualCollection>($"message");
        msg.Formatter = new DpMsgFormatter();
        var frm = new ExplorerForm();
        frm.LoadMessage(msg);
        frm.Show();
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
                    var path = Project.GetPath(relpaths[i]);
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

    //[Dump]
    //public IEnumerable<string> DumpFormattedTexts()
    //{
    //    var formatter = new TextFormatter();
    //    formatter.SymbolReplacement.Add(0xE300, "₽"); // PokeDoller mark

    //    formatter.NewLineMarker = new[] {
    //        @"[0xFFFE]",
    //        @"{0xBE00}",
    //        @"{0xBE01}",
    //    };

    //    foreach (var group in new string[] { "common", "script" })
    //    {
    //        var outputFolder = Path.Combine(OutputPath, "message_formatted", "jp", group);
    //        Directory.CreateDirectory(outputFolder);

    //        var data = GetData<PokemonText[]>(group);
    //        for (var i = 0; i < data.Length; i++)
    //        {
    //            var outputFile = Path.Combine(outputFolder, $"{i}.json");
    //            var options = new FormatOptions() { Format = FormatType.Plain };
    //            var lines = data[i].Entries.Select(x => formatter.Format(x, options));
    //            //Serializer.WriteLF(outputFile, lines);
    //            yield return outputFile;
    //        }
    //    }
    //}
}
