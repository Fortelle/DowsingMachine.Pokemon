using FlatSharp.Attributes;
using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;
using PBT.DowsingMachine.Pokemon.Games;
using PBT.DowsingMachine.Projects;
using SoulsFormats;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

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

        AddReference($"message", new MessageReaderSV(@$"trpfs", LanguageMaps));
    }


    [Extraction]
    public IEnumerable<string> ExtractTrpfs()
    {
        var trpfd = GetData<TRPFD>($"trpfd");
        var trpfs = GetData<TRPFS>($"trpfs");

        var outputFolder = Path.Combine(Root, "trpfs");

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

}
