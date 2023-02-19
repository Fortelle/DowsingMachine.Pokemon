using GFMSG;
using GFMSG.Pokemon;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Pokemon.Games;
using PBT.DowsingMachine.Projects;
using System.ComponentModel;

namespace PBT.DowsingMachine.Pokemon.Core.Gen4;

public class PokemonProjectIV : PokemonProjectDS, IPreviewString
{
    [Option]
    [TypeConverter(typeof(EnumSelectConverter))]
    [Select(GameTitle.Diamond, GameTitle.Pearl, GameTitle.Platinum, GameTitle.HeartGold, GameTitle.SoulSilver)]
    public override GameTitle Title { get; set; }

    public int InternalPokemonCount = 501;

    public PokemonProjectIV() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        switch (Title)
        {
            case GameTitle.Diamond:
            case GameTitle.Pearl:
                Resources.Add(new DataResource("pokemon_personals")
                {
                    Reference = new FileRef(@"root\poketool\personal\personal.narc"),
                    Reader = new NarcReader()
                        .Then(MarshalArray<Personal4>)
                });
                Resources.Add(new DataResource("pokemon_levelup_moves")
                {
                    Reference = new FileRef(@"root\poketool\personal\wotbl.narc"),
                    Reader = new NarcReader()
                        .Then(ParseEnumerable(ReadLevelupMoves))
                });
                Resources.Add(new DataResource("msg")
                {
                    Reference = new FileRef(@"root\msgdata\msg.narc"),
                    Reader = new NarcReader()
                        .Then(ParseEnumerable(x => new MsgDataV1(x)))
                });
                Resources.Add(new DataResource("tmhm_move_list")
                {
                    Reference = new FileRef(@"root\ftc\arm9.bin"),
                    Reader = new FileReader(0xFA318)
                        .Then(br => br.ReadShorts(100))
                });
                Resources.Add(new DataResource("pokemon_egg_moves")
                {
                    Reference = new FileRef(@"root\ftc\overlay9_5"),
                    Reader = new OverlayReader(0x21648)
                        .Then(ReadEggMoves)
                });
                break;
            case GameTitle.Platinum:
                Resources.Add(new DataResource("pokemon_personals")
                {
                    Reference = new FileRef(@"root\poketool\personal\pl_personal.narc"),
                    Reader = new NarcReader()
                        .Then(MarshalArray<Personal4>)
                });
                Resources.Add(new DataResource("pokemon_levelup_moves")
                {
                    Reference = new FileRef(@"root\poketool\personal\wotbl.narc"),
                    Reader = new NarcReader()
                        .Then(ParseEnumerable(ReadLevelupMoves))
                });
                Resources.Add(new DataResource("msg")
                {
                    Reference = new FileRef(@"root\msgdata\msg.narc"),
                    Reader = new NarcReader()
                        .Then(ParseEnumerable(x => new MsgDataV1(x)))
                });
                Resources.Add(new DataResource("tmhm_move_list")
                {
                    Reference = new FileRef(@"root\ftc\arm9.bin"),
                    Reader = new FileReader(0xF028C)
                        .Then(br => br.ReadShorts(100))
                });
                Resources.Add(new DataResource("pokemon_egg_moves")
                {
                    Reference = new FileRef(@"root\ftc\overlay9_5"),
                    Reader = new OverlayReader(0x29012)
                        .Then(ReadEggMoves)
                });
                Resources.Add(new DataResource("pokemon_tutor_moves")
                {
                    Reference = new FileRef(@"root\ftc\overlay9_5"),
                    Reader = new OverlayReader(0x2FF1C)
                        .Then(br => br.ReadByteMatrix(5, 507))
                        .Then(ParseEnumerable(x => new FlagArray(x)))
                });
                Resources.Add(new DataResource("tutor_move_data")
                {
                    Reference = new FileRef(@"root\ftc\overlay9_5"),
                    Reader = new OverlayReader(0x2FD54)
                        .Then(br => br.ReadByteMatrix(12, 38))
                        .Then(MarshalArray<PlatinumWazaOshieList>)
                });
                break;
            case GameTitle.HeartGold:
            case GameTitle.SoulSilver:
                Resources.Add(new DataResource("pokemon_personals")
                {
                    Reference = new FileRef(@"root\a\0\0\2"),
                    Reader = new NarcReader()
                        .Then(MarshalArray<Personal4>)
                });
                Resources.Add(new DataResource("pokemon_levelup_moves")
                {
                    Reference = new FileRef(@"root\a\0\3\3"),
                    Reader = new NarcReader()
                        .Then(ParseEnumerable(ReadLevelupMoves))
                });
                Resources.Add(new DataResource("msg")
                {
                    Reference = new FileRef(@"root\a\0\2\7"),
                    Reader = new NarcReader()
                        .Then(ParseEnumerable(x => new MsgDataV1(x)))
                });
                Resources.Add(new DataResource("tmhm_move_list")
                {
                    Reference = new FileRef(@"root\ftc\arm9_decompressed.bin"),
                    Reader = new FileReader(0x000FF84C)
                        .Then(br => br.ReadShorts(100))
                });
                Resources.Add(new DataResource("pokemon_egg_moves")
                {
                    Reference = new FileRef(@"root\data\kowaza.narc"),
                    Reader = new NarcReader()
                        .Then(narc => narc[0])
                        .Then(x => new BinaryReader(new MemoryStream(x)))
                        .Then(ReadEggMoves)
                });
                Resources.Add(new DataResource("pokemon_tutor_moves")
                {
                    Reference = new FileRef(@"root\fielddata\wazaoshie\waza_oshie.bin"),
                    Reader = new FileReader()
                        .Then(br => br.ReadByteMatrix(8))
                        .Then(ParseEnumerable(x => new FlagArray(x)))
                });
                Resources.Add(new DataResource("tutor_move_data")
                {
                    Reference = new FileRef(@"root\ftc\overlay9_1"),
                    Reader = new OverlayReader(0x00023954)
                        .Then(br => br.ReadByteMatrix(4, 52))
                        .Then(MarshalArray<HGSSTutorMove>)
                });
                break;
        }
    }

    private static LevelupMove[] ReadLevelupMoves(BinaryReader br)
    {
        var list = new List<LevelupMove>();
        while (true)
        {
            var value = br.ReadUInt16();
            if (value == 0xFFFF) break;
            var lv = value >> 9;
            var mi = value & 0b111111111;
            list.Add(new LevelupMove(mi, lv));
        }
        return list.ToArray();
    }

    private Dictionary<int, int[]> ReadEggMoves(BinaryReader br)
    {
        var dict = new Dictionary<int, List<int>>();
        List<int> list = null;
        while (true)
        {
            var value = br.ReadUInt16();
            if (value == 0xFFFF)
            {
                break;
            }
            else if (value > 20000)
            {
                list = new List<int>();
                dict.Add(value - 20000, list);
            }
            else
            {
                list.Add(value);
            }
        }
        return dict.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }

    [Data]
    public MultilingualCollection DumpMessage()
    {
        var langcode = Language switch
        {
            "jpn" => "ja-Hrkt",
            "eng" => "en-US",
            "fra" => "fr",
            "ita" => "it",
            "ger" => "de",
            "spa" => "es",
            "kor" => "ko",
            _ => ""
        };

        var messages = GetData<MsgDataV1[]>("msg");

        var wrappers = messages.Select(msg =>
        {
            var fn = msg.Seed.ToString();
            var mw = new MsgWrapper(msg, fn, langcode)
            {
                Version = FileVersion.GenIV,
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

        var mc = new MultilingualCollection
        {
            Version = FileVersion.GenIV,
            Formatter = new DpMsgFormatter(),
        };
        mc.Wrappers.Add(Language ?? "", wrappers);
        return mc;
    }

    #region "Message"
    public string GetString(string filename, int value) => GetOrCreateCache(DumpMessage).GetString(null, filename, value);

    public string[] GetStrings(string filename) => GetOrCreateCache(DumpMessage).GetStrings(null, filename);

    public string GetPreviewString(params object[] args) => GetString($"{args[0]}", Convert.ToInt32(args[1]));
    #endregion


    // poketool/poke_tool.c => int PokeOtherFormMonsNoGet(int mons_no,int form_no)
    public static PokemonId[] GetPokemonIds(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => i switch
            {
                494 => new PokemonId(), // egg
                495 => new PokemonId(), // manaphy egg
                496 or 497 or 498 => new PokemonId(386, i - 496 + 1), // deoxys
                499 or 500 => new PokemonId(413, i - 499 + 1), // wormadam
                501 => new PokemonId(487, i - 501 + 1),
                502 => new PokemonId(492, i - 502 + 1),
                503 or 504 or 505 or 506 or 507 => new PokemonId(479, i - 503 + 1),
                _ => new PokemonId(i)
            })
            .ToArray();
    }

    // field/scr_wazaoshie.c => GetWazaOshieDataAdrs
    public static PokemonId[] GetWazaOshieIds(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => i switch
            {
                494 or 495 or 496 => new PokemonId(386, i - 494 + 1), // deoxys
                497 or 498 => new PokemonId(413, i - 497 + 1), // wormadam
                499 => new PokemonId(487, i - 499 + 1),
                500 => new PokemonId(492, i - 500 + 1),
                501 or 502 or 503 or 504 or 505 => new PokemonId(479, i - 501 + 1),
                _ => new PokemonId(i)
            })
            .ToArray();
    }

    [Data("learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = GetData<Personal4[]>("pokemon_personals");
        var dexNumbers = GetPokemonIds(personals.Length);
        var levelup = GetData<LevelupMove[][]>("pokemon_levelup_moves");
        var tmlist = GetData<short[]>("tmhm_move_list");
        var eggs = GetData<Dictionary<int, int[]>>("pokemon_egg_moves");

        var collection = new LearnsetTableCollection("{0:000}.{1:00}");

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < personals.Length; i++)
            {
                var data = levelup[i].Select(x => $"{x.Move}:{x.Level}").ToArray();
                lt.Add(dexNumbers[i], data);
            }
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < personals.Length; i++)
            {
                var flags = new FlagArray(personals[i].Machine1, personals[i].Machine2, personals[i].Machine3, personals[i].Machine4);
                var data = flags.OfTrue(tmlist, (m, j) => j < 92 ? $"{m}:TM{j + 1:00}" : $"{m}:HM{j - 91:00}");
                lt.Add(dexNumbers[i], data);
            }
            collection.Add("tm", lt);
        }

        {
            var lt = new LearnsetTable
            {
                // fielddata/script/r228r0201.ev => ev_r228r0201_oldman1
                { new PokemonId(6), 307 },
                { new PokemonId(157), 307 },
                { new PokemonId(257), 307 },
                { new PokemonId(392), 307 },

                { new PokemonId(9), 308 },
                { new PokemonId(160), 308 },
                { new PokemonId(260), 308 },
                { new PokemonId(395), 308 },

                { new PokemonId(3), 338 },
                { new PokemonId(154), 338 },
                { new PokemonId(254), 338 },
                { new PokemonId(389), 338 }
            };

            // fielddata/script/r210br0101.ev => ev_r210br0101_dragon
            var DRAGON_TYPE = 16;
            for (var i = 0; i < personals.Length; i++)
            {
                if (personals[i].Type1 == DRAGON_TYPE || personals[i].Type2 == DRAGON_TYPE)
                {
                    lt.Add(dexNumbers[i], 434);
                }
            }
            lt.Add(new PokemonId(493, DRAGON_TYPE), 434);

            collection.Add("tutor_ult", lt);
        }

        if (Game.Title is GameTitle.Platinum)
        {
            var flags = GetData<FlagArray[]>("pokemon_tutor_moves");
            var tutors = GetData<PlatinumWazaOshieList[]>("tutor_move_data").Select(x => x.Waza).ToArray();
            var number = GetWazaOshieIds(flags.Length);

            var lt = new LearnsetTable();
            for (var i = 0; i < flags.Length; i++)
            {
                var data = flags[i].OfTrue(tutors, x => x.ToString());
                lt.Add(number[i], data);
            }
            collection.Add("tutor", lt);
        }
        else if (Game.Title is GameTitle.HeartGold or GameTitle.SoulSilver)
        {
            var tutors = GetData<HGSSTutorMove[]>("tutor_move_data").Select(x => x.Waza).ToArray();
            var flags = GetData<FlagArray[]>("pokemon_tutor_moves");
            var number = GetWazaOshieIds(flags.Length);

            var lt = new LearnsetTable();
            for (var i = 0; i < flags.Length; i++)
            {
                var data = flags[i].OfTrue(tutors, x => x.ToString());
                lt.Add(number[i], data);
            }
            collection.Add("tutor", lt);
        }


        {
            var lt = new LearnsetTable();
            foreach (var (index, moves) in eggs)
            {
                lt.Add(dexNumbers[index], moves);
            }
            //lt.Append(new PokemonId(175), 344);
            collection.Add("egg", lt);
        }

        {
            // hardcoded moves
            var lt = new LearnsetTable();
            // field/sodateya.c => PichuExtraCheck
            lt.Add(new PokemonId(175), 344);
            if (Game.Title is GameTitle.Platinum or GameTitle.HeartGold or GameTitle.SoulSilver)
            {
                // rotom: https://github.com/pret/pokeheartgold/blob/cec81057d49fd9f95515166ee92311a49c02d564/src/pokemon.c#L3627
                var rotom_form_moves = new[] { 0, 315, 56, 59, 403, 437 };
                for (var i = 1; i < 6; i++)
                {
                    lt.Add(new PokemonId(479, i), rotom_form_moves[i]);
                }
            }
            collection.Add("special", lt);
        }

        return collection;
    }
}
