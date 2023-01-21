using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core;

namespace PBT.DowsingMachine.Pokemon.Core.Gen5;

public class PokemonDreamRadar : PokemonProjectDS
{
    private static readonly Dictionary<string, string> LanguageMap = new()
    {
        ["ja-Hrkt"] = "JP_Japanese",
        ["en"] = "English",
        ["fr"] = "French",
        ["it"] = "Italian",
        ["de"] = "German",
        ["es"] = "Spanish",
        ["ko"] = "Korean",
    };

    public PokemonDreamRadar() : base()
    {
        /*
        foreach (var (lang, name) in LanguageMap)
        {
            AddReference(lang,
                new ArchiveReader<CARC>(@$"Message\{name}\MsgData.carc"),
                x => new DARC(x.Data),
                x => x.Entries.Select(x => x.New(new MSBT(x.Data))).ToArray()
                );
        }
        */
    }


}
