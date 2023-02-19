using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using PBT.DowsingMachine.Projects;
using System.Security.Cryptography;

namespace PBT.DowsingMachine.Pokemon.Core;

public abstract class PokemonProjectTrinity : ExtendableProject, IPokemonProject
{
    [Option]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }

    protected PokemonProjectTrinity() : base()
    {
        Resources.Add(new DataResource("data.trpfd")
        {
            Browsable = false,
            Reference = new FileRef(@"romfs\arc\data.trpfd"),
            Reader = new DataReader<string>()
                .Then(FlatBufferConverter.DeserializeFrom<TRPFD>),
        });
        Resources.Add(new DataResource("data.trpfs")
        {
            Browsable = false,
            Reference = new FileRef(@"romfs\arc\data.trpfs"),
            Reader = new FileReader<TRPFS>(),
        });

    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);
    }

    public string GetPreviewString(object[] args)
    {
        throw new NotImplementedException();
    }


    protected override object ReadReference(IDataReference reference, GetDataOptions options)
    {
        switch (reference)
        {
            case TrinityRef trpfsRef:
                {
                    var filename = trpfsRef.Filename;
                    if( options.ReferenceArguments?.Length > 0)
                    {
                        filename = string.Format(filename, options.ReferenceArguments);
                        options.CacheKey += "_" + filename;
                    }

                    byte[] data = null;
                    {
                        var trpfs = GetData<TRPFS>("data.trpfs");
                        var trpfd = GetData<TRPFD>("data.trpfd"); 
                        var packname = trpfd.FindPackName(filename);
                        if (packname != null)
                        {
                            data = trpfs[packname];
                        }
                    }
                    if (data == null && IsExtendable && OriginalProject != null)
                    {
                        var trpfs = OriginalProject.GetData<TRPFS>("data.trpfs");
                        var trpfd = OriginalProject.GetData<TRPFD>("data.trpfd");
                        var packname = trpfd.FindPackName(filename);
                        if (packname != null)
                        {
                            data = trpfs[packname];
                        }
                    }
                    if (data is null) return null;
                    var trpak = Trpak.Load(data);
                    return trpak[filename];
                }
        }

        return base.ReadReference(reference, options);
    }


    public class TrinityRef : DataRef<byte[]>
    {
        public string Filename;

        public override string Description => Filename;

        public TrinityRef(string filename)
        {
            Filename = filename;
        }

    }

    [Action]
    public IEnumerable<string> ExtractTrpfs()
    {
        var trpfd = GetData<TRPFD>($"data.trpfd");
        var trpfs = GetData<TRPFS>($"data.trpfs");

        for (var i = 0; i < trpfd.PackNames.Length; i++)
        {
            var hash = Utilities.Codecs.FnvHash.Fnv1a_64(trpfd.PackNames[i]);
            var data = trpfs[hash];
            var filepath = Path.Combine(OutputFolder, "trpfs", trpfd.PackNames[i]);
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));
            File.WriteAllBytes(filepath, data);
            yield return filepath;
        }
    }

    [Action]
    public IEnumerable<string> ExtractTrpak()
    {
        var trpfd = GetData<TRPFD>($"data.trpfd");
        var trpfs = GetData<TRPFS>($"data.trpfs");

        for (var i = 0; i < trpfd.PackNames.Length; i++)
        {
            var hash = Utilities.Codecs.FnvHash.Fnv1a_64(trpfd.PackNames[i]);
            var data = trpfs[hash];

            var folder = Path.Combine(OutputFolder, "trpak", trpfd.PackNames[i]);
            Directory.CreateDirectory(folder);

            var trpak = Trpak.Load(data);
            for (var j = 0; j < trpak.Entries.Length; j++)
            {
                var filename = trpak.Hashes[j].ToString("X16");
                var path = Path.Combine(folder, filename);
                var filedata = trpak[j];
                File.WriteAllBytes(path, filedata);
            }
            yield return folder;
        }
    }

}
