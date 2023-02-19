using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.Gen2;
using PBT.DowsingMachine.Projects;
using System.ComponentModel;
using System.Text;

namespace PBT.DowsingMachine.Pokemon.Core.Gen3;

public class PokemonProjectIII : FileProject, IPokemonProject
{
    [Option]
    [TypeConverter(typeof(EnumSelectConverter))]
    [Select(GameTitle.Ruby, GameTitle.Sapphire, GameTitle.FireRed, GameTitle.LeafGreen, GameTitle.Emerald)]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }


    public PokemonProjectIII() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);

        var variables = new
        {
            internal_pokemon_count = 412,
            personal_byte_count = 28,
            tmhm_move_count = 57,
            tmhm_byte_count = 8,

            pokemon_personals = 0u,
            pokemon_dex_number = 0u,
            pokemon_hoenn_dex_number = 0u,
            pokemon_levelup_moves = 0u,
            pokemon_tmhm_moves = 0u,
            pokemon_egg_moves = 0u,
            pokemon_tutor_moves = 0u,
            tmhm_move_list = 0u,
            tutor_move_list = 0u,

            tutor_move_count = 0,
            tutor_byte_count = 0,
        };


        switch (Title)
        {
            case GameTitle.Ruby:
            case GameTitle.Sapphire:
                variables = variables with
                {
                    pokemon_personals = 0x1D09CC,
                    pokemon_dex_number = 0x1CE2CA,
                    pokemon_hoenn_dex_number = 0x1CDF94,
                    pokemon_levelup_moves = 0x1D997C,
                    pokemon_tmhm_moves = 0x1CEEA4,
                    pokemon_egg_moves = 0x1DAF78,
                    tmhm_move_list = 0x35017C,
                };
                break;
            case GameTitle.FireRed:
                variables = variables with
                {
                    pokemon_personals = 0x21118C,
                    pokemon_dex_number = 0x20E9F6,
                    pokemon_hoenn_dex_number = 0x20E6C0,
                    pokemon_levelup_moves = 0x21A1BC,
                    pokemon_tmhm_moves = 0x20F5D0,
                    pokemon_egg_moves = 0x21B918,
                    pokemon_tutor_moves = 0x41930E,
                    tmhm_move_list = 0x419D34,
                    tutor_move_list = 0x4192F0,
                    tutor_move_count = 16,
                    tutor_byte_count = 2,
                };
                break;
            case GameTitle.LeafGreen:
                variables = variables with
                {
                    pokemon_personals = 0x21118C,
                    pokemon_dex_number = 0x20E9D2,
                    pokemon_hoenn_dex_number = 0x20E69C,
                    pokemon_levelup_moves = 0x21A19C,
                    pokemon_tmhm_moves = 0x20F5AC,
                    pokemon_egg_moves = 0x21B8F8,
                    pokemon_tutor_moves = 0x419296,
                    tmhm_move_list = 0x419CBC,
                    tutor_move_list = 0x419278,
                    tutor_move_count = 16,
                    tutor_byte_count = 2,
                };
                break;
            case GameTitle.Emerald:
                variables = variables with
                {
                    pokemon_personals = 0x2F0D54,
                    pokemon_dex_number = 0x2EE60A,
                    pokemon_hoenn_dex_number = 0x2EE2D4,
                    pokemon_levelup_moves = 0x2F9D04,
                    pokemon_tmhm_moves = 0x2EF220,
                    pokemon_egg_moves = 0x2FB764,
                    pokemon_tutor_moves = 0x5E0900,
                    tmhm_move_list = 0x5E144C,
                    tutor_move_list = 0x5E08C4,
                    tutor_move_count = 32,
                    tutor_byte_count = 4,
                };
                break;
        }


        Resources.Add(new DataResource("pokemon_personals")
        {
            Reference = new PosRef(variables.pokemon_personals),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadByteMatrix(variables.personal_byte_count, variables.internal_pokemon_count))
                .Then(MarshalArray<Personal3>)
        });
        Resources.Add(new DataResource("pokemon_dex_number")
        {
            Reference = new PosRef(variables.pokemon_dex_number),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadShorts(variables.internal_pokemon_count - 1)) // starts from 1
        });
        Resources.Add(new DataResource("pokemon_hoenn_dex_number")
        {
            Reference = new PosRef(variables.pokemon_hoenn_dex_number),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadShorts(variables.internal_pokemon_count - 1)) 
        });
        Resources.Add(new DataResource("tmhm_move_list")
        {
            Reference = new PosRef(variables.tmhm_move_list),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadShorts(variables.tmhm_move_count))
        });
        Resources.Add(new DataResource("pokemon_levelup_moves")
        {
            Reference = new PosRef(variables.pokemon_levelup_moves),
            Reader = new DataReader<BinaryReader>()
                .Then(ReadPokemonLevelupMoves)
        });
        Resources.Add(new DataResource("pokemon_egg_moves")
        {
            Reference = new PosRef(variables.pokemon_egg_moves),
            Reader = new DataReader<BinaryReader>()
                .Then(ReadPokemonEggMoves)
        });
        Resources.Add(new DataResource("pokemon_tmhm_moves")
        {
            Reference = new PosRef(variables.pokemon_tmhm_moves),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadByteMatrix(variables.tmhm_byte_count, variables.internal_pokemon_count))
                .Then(ParseEnumerable(x => new FlagArray(x)))
        });

        Resources.Add(new DataResource("tutor_move_list")
        {
            Enable = variables.tutor_move_list > 0,
            Reference = new PosRef(variables.tutor_move_list),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadShorts(variables.tutor_move_count))
        });
        Resources.Add(new DataResource("pokemon_tutor_moves")
        {
            Enable = variables.pokemon_tutor_moves > 0,
            Reference = new PosRef(variables.pokemon_tutor_moves),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadByteMatrix(variables.tutor_byte_count, variables.internal_pokemon_count))
                .Then(ParseEnumerable(x => new FlagArray(x)))
        });

    }


    private LevelupMove[][] ReadPokemonLevelupMoves(BinaryReader br)
    {
        var offsets = Enumerable.Range(0, 412).Select(_ => br.ReadInt32()).ToArray();

        var list = new List<LevelupMove[]>();
        foreach (var offset in offsets)
        {
            br.BaseStream.Seek(offset - 0x08000000, SeekOrigin.Begin);
            var ms = new List<LevelupMove>();
            while (true)
            {
                var value = br.ReadUInt16();
                if (value == 0xFFFF) break;
                var lv = value >> 9;
                var mi = value & 0b111111111;
                ms.Add(new LevelupMove(mi, lv));
            }
            list.Add(ms.ToArray());
        }

        return list.ToArray();
    }

    private Dictionary<int, int[]> ReadPokemonEggMoves(BinaryReader br)
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


    [Test]
    public PokemonId[] GetPokemonIds()
    {
        return GetData<short[]>("pokemon_dex_number")
            .Select(x => x switch 
            {
                386 when Title is GameTitle.FireRed => new PokemonId(386, 1),
                386 when Title is GameTitle.LeafGreen => new PokemonId(386, 2),
                386 when Title is GameTitle.Emerald => new PokemonId(386, 3),
                > 386 => PokemonId.Empty,
                _ => new PokemonId(x)
            })
            .Prepend(PokemonId.Empty)
            .ToArray();
    }

    [Test]
    public (PokemonId, Personal3)[] GetKeyedPersonals()
    {
        return GetData<Personal3[]>("pokemon_personals")
            .Zip(GetPokemonIds(), (per, id) => (id, per))
            .ToArray();
    }

    [Test]
    public (PokemonId, LevelupMove[])[] GetKeyedPokemonLevelupMoves()
    {
        return GetData<LevelupMove[][]>("pokemon_levelup_moves")
            .Zip(GetPokemonIds())
            .Select(x => (x.Second, x.First))
            .ToArray();
    }

    [Test]
    public (PokemonId, int[])[] GetKeyedPokemonEggMoves()
    {
        var ids = GetPokemonIds();
        return GetData<Dictionary<int, int[]>>("pokemon_egg_moves")
            .Select(x => (ids[x.Key], x.Value))
            .ToArray();
    }

    [Test]
    public (PokemonId, FlagArray)[] GetKeyedPokemonTmMoves()
    {
        return GetData<FlagArray[]>("pokemon_tmhm_moves")
            .Zip(GetPokemonIds())
            .Select(x => (x.Second, x.First))
            .ToArray();
    }

    [Test]
    [Title(GameTitle.Emerald, GameTitle.FireRed, GameTitle.LeafGreen)]
    public (PokemonId, FlagArray)[] GetKeyedPokemonTutorMoves()
    {
        return GetData<FlagArray[]>("pokemon_tutor_moves")
            .Zip(GetPokemonIds())
            .Select(x => (x.Second, x.First))
            .ToArray();
    }

    [Data(@"learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var ids = GetPokemonIds();
        var levelups = GetKeyedPokemonLevelupMoves();
        var tmhms = GetKeyedPokemonTmMoves();
        var tmList = GetData<short[]>("tmhm_move_list");
        var eggs = GetKeyedPokemonEggMoves();

        var collection = new LearnsetTableCollection(@"{0:000}.{1:00}");

        {
            var lt = new LearnsetTable();
            foreach (var (id, levelup) in levelups)
            {
                var data = levelup.Select(x => $"{x.Move}:{x.Level}").ToArray();
                lt.Add(id, data);
            }
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, tm) in tmhms)
            {
                var data = tm.OfTrue(tmList, (m, j) => j < 50 ? $"{m}:TM{j + 1:00}" : $"{m}:HM{j - 49:00}").ToArray();
                lt.Add(id, data);
            }
            collection.Add("tm", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, moves) in eggs)
            {
                lt.Add(id, moves);
            }
            collection.Add("egg", lt);
        }

        if (Game.Title is GameTitle.FireRed or GameTitle.LeafGreen or GameTitle.Emerald)
        {
            var tutorFlags = GetKeyedPokemonTutorMoves();
            var tutorList = GetData<short[]>("tutor_move_list");

            var lt = new LearnsetTable();
            foreach (var (id, moves) in tutorFlags)
            {
                var data = moves.OfTrue(tutorList, (m) => $"{m}").ToArray();
                lt.Add(id, data);
            }

            collection.Add("tutor", lt);
        }

        // hardcoded
        if (Game.Title is GameTitle.FireRed or GameTitle.LeafGreen)
        {
            var lt = new LearnsetTable()
            {
                { new PokemonId(3), 338 },
                { new PokemonId(6), 307 },
                { new PokemonId(9), 308 }
            };
            collection.Add("tutor_ult", lt);
        }

        if (Game.Title is GameTitle.Emerald)
        {
            var lt = new LearnsetTable()
            {
                { new PokemonId(175), 344 }
            };
            collection.Add("special", lt);
        }

        return collection;
    }


    [Action]
    [Title(GameTitle.Ruby, GameTitle.Sapphire)]
    public string CompareWithGSC()
    {
        var old = DowsingMachineApp.FindProject<PokemonProjectII>(s => s
            .MaxBy(p => p.Title)
            );
        if (old is null) return "";

        var sb = new StringBuilder();

        {
            var newLearnsets = DumpLearnsets();
            var oldLearnsets = old.DumpLearnsets();
            var diffs = newLearnsets.CompareWith(oldLearnsets);
            foreach (var diff in diffs)
            {
                sb.AppendLine(diff.Pokemon.ToString("No.{0:000}"));
                sb.AppendLine(diff.ToText());
            }
        }

        return sb.ToString();
    }

    [Action]
    [Title(GameTitle.Emerald)]
    public string CompareWithRS()
    {
        var old = DowsingMachineApp.FindProject<PokemonProjectIII>(s => s
            .Where(p => p.Title is GameTitle.Ruby or GameTitle.Sapphire)
            .MaxBy(p => p.Title)
            );
        if (old is null) return "";

        var sb = new StringBuilder();

        {
            var oldPersonals = old.GetKeyedPersonals().Where(x => x.Item1.IsValid).ToDictionary(x => x.Item1, x => x.Item2);
            var newPersonals = GetKeyedPersonals().Where(x => x.Item1.IsValid).ToDictionary(x => x.Item1, x => x.Item2);

            var ch = new DictionaryComparer<PokemonId>()
            {
                KeyToString = (x) => $"No.{x.Number:000} {(Monsname)x.Number}",
                IgnoreProperties = new[]
                {
                    "Machine1",
                    "Machine2",
                },
            };
            sb.AppendLine("Pokemon");
            sb.AppendLine("----------");
            sb.AppendLine(ch.Compare(oldPersonals, newPersonals));
        }

        {
            var newLearnsets = DumpLearnsets();
            var oldLearnsets = old.DumpLearnsets();
            var diffs = newLearnsets.CompareWith(oldLearnsets, "tm");
            foreach (var diff in diffs)
            {
                sb.AppendLine(diff.Pokemon.ToString("No.{0:000}"));
                sb.AppendLine(diff.ToText());
            }
        }

        return sb.ToString();
    }

}
