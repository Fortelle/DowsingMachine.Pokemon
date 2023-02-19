using GFMSG;
using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.Gen8;
using PBT.DowsingMachine.Projects;
using System.ComponentModel;
using System.Text;

namespace PBT.DowsingMachine.Pokemon.Core.Gen1;

public class PokemonProjectI : FileProject, IPokemonProject
{
    [Option]
    [TypeConverter(typeof(EnumSelectConverter))]
    [Select(GameTitle.Red, GameTitle.Green, GameTitle.Blue, GameTitle.Yellow, GameTitle.RedVC, GameTitle.GreenVC, GameTitle.BlueVC, GameTitle.YellowVC)]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }

    public const int InternalPokemonCount = 0xBE;

    public PokemonProjectI() : base()
    {
    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);

        var positions = new
        {
            tmhm_move_list = 0u,
            pokemon_evolution_table = 0u,
            pokemon_dex_number = 0u,
            pokemon_personal_150 = 0u,
            pokemon_personal_mew = 0u,
            pokemon_personal_151 = 0u,
        };

        switch (Title)
        {
            case GameTitle.Red:
            case GameTitle.Green:
            case GameTitle.Blue:
                positions = positions with
                {
                    tmhm_move_list = 0x12276,
                    pokemon_evolution_table = 0x3B427,
                    pokemon_dex_number = 0x4279A,
                    pokemon_personal_150 = 0x38000,
                    pokemon_personal_mew = 0x4200,
                };
                break;
            case GameTitle.Yellow:
                positions = positions with
                {
                    tmhm_move_list = 0x1286C,
                    pokemon_evolution_table = 0x3B59C,
                    pokemon_dex_number = 0x4282D,
                    pokemon_personal_151 = 0x383DE,
                };
                break;
            case GameTitle.RedVC:
            case GameTitle.BlueVC:
                positions = positions with
                {
                    tmhm_move_list = 0x13773,
                    pokemon_evolution_table = 0x3B05C,
                    pokemon_dex_number = 0x41024,
                    pokemon_personal_150 = 0x383DE,
                    pokemon_personal_mew = 0x425B,
                };
                break;
        }

        if (IsRGB)
        {
            Resources.Add(new DataResource("pokemon_personal_150")
            {
                Reference = new PosRef(positions.pokemon_personal_150),
                Reader = new DataReader<BinaryReader>()
                    .Then(br => br.ReadByteMatrix(28, 150))
                    .Then(MarshalArray<Personal1>)
            });
            Resources.Add(new DataResource("pokemon_personal_mew")
            {
                Reference = new PosRef(positions.pokemon_personal_mew),
                Reader = new DataReader<BinaryReader>()
                    .Then(br => br.ReadByteMatrix(28, 1))
                    .Then(MarshalArray<Personal1>)
            });
        }
        else
        {
            Resources.Add(new DataResource("pokemon_personal_151")
            {
                Reference = new PosRef(positions.pokemon_personal_151),
                Reader = new DataReader<BinaryReader>()
                    .Then(br => br.ReadByteMatrix(28, 151))
                    .Then(MarshalArray<Personal1>)
            });
        }

        // 0-49 for tm, 50-54 for hm
        Resources.Add(new DataResource("tmhm_move_list")
        {
            Reference = new PosRef(positions.tmhm_move_list),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadBytes(55))
                .Then(bytes => bytes.ToIntegers())
        });
        Resources.Add(new DataResource("pokemon_evolution_table")
        {
            Reference = new PosRef(positions.pokemon_evolution_table),
            Reader = new DataReader<BinaryReader>()
                .Then(ReadPokemonEvolutions)
        });
        Resources.Add(new DataResource("pokemon_dex_number")
        {
            Reference = new PosRef(positions.pokemon_dex_number),
            Reader = new DataReader<BinaryReader>()
                .Then(br => br.ReadBytes(InternalPokemonCount))
                .Then(bytes => bytes.ToIntegers())
        });

    }

    public bool IsRGB => Title is GameTitle.Red
        or GameTitle.Green
        or GameTitle.Blue
        or GameTitle.RedVC
        or GameTitle.BlueVC
        or GameTitle.GreenVC;

    public bool IsVC => Title is GameTitle.RedVC
        or GameTitle.BlueVC
        or GameTitle.GreenVC
        or GameTitle.YellowVC;


    private static Evolution[] ReadPokemonEvolutions(BinaryReader br)
    {
        var offsets = br.ReadUShorts(InternalPokemonCount);

        var list = new List<Evolution>();
        foreach (var offset in offsets)
        {
            br.BaseStream.Seek(offset + 0x034000, SeekOrigin.Begin);

            var evo = new Evolution()
            {
                Methods = new(),
                Moves = new(),
            };

            while (true)
            {
                var ev = br.ReadByte();
                if (ev == 0) break;
                var data = new Dictionary<string, int>
                {
                    { "condition", ev }
                };
                if (ev == 2)
                {
                    data.Add("item", br.ReadByte());
                }
                data.Add("level", br.ReadByte());
                data.Add("mons", br.ReadByte());
                evo.Methods.Add(data);
            }

            while (true)
            {
                var lv = br.ReadByte();
                if (lv > 0)
                {
                    var mi = br.ReadByte();
                    evo.Moves.Add(new LevelupMove(mi, lv));
                }
                else
                {
                    break;
                }
            }

            list.Add(evo);
        }

        return list.ToArray();
    }

    [Test]
    public (PokemonId, Personal1)[] GetKeyedPersonals()
    {
        var personals = IsRGB
            ? GetData<Personal1[]>("pokemon_personal_150").Concat(GetData<Personal1[]>("pokemon_personal_mew"))
            : GetData<Personal1[]>("pokemon_personal_151");
        return personals
            .Select((x, i) => (new PokemonId(x.OldNumber), x))
            .ToArray();
    }

    [Data(@"learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = GetKeyedPersonals();
        var tmhmlist = GetData<int[]>("tmhm_move_list");
        var evo = GetData<Evolution[]>("pokemon_evolution_table");
        var dexNumbers = GetData<int[]>("pokemon_dex_number");

        var collection = new LearnsetTableCollection(@"{0:000}");

        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var data = new int[] { personal.Skill1, personal.Skill2, personal.Skill3, personal.Skill4 }.Where(x => x > 0).ToArray();
                lt.Add(id, data);
            }
            collection.Add("basic", lt);
        }

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < evo.Length; i++)
            {
                var data = evo[i].Moves.Select(x => $"{x.Move}:{x.Level}").ToArray();
                lt.Add(new PokemonId(dexNumbers[i]), data);
            }
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            foreach (var (id, personal) in personals)
            {
                var fa = new FlagArray(personal.Machine1, personal.Machine2);
                var data = fa.OfTrue(tmhmlist, (m, j) => j < 50 ? $"{m}:TM{j + 1:00}" : $"{m}:HM{j - 49:00}");
                lt.Add(id, data);
            }
            collection.Add("tm", lt);
        }

        return collection;
    }

    [Action]
    [Title(GameTitle.Yellow)]
    public string CompareWithRGB()
    {
        using var rgb = DowsingMachineApp.FindProject<PokemonProjectI>(s => s
            .OfType<PokemonProjectI>()
            .Where(x => x != this && x.IsRGB)
            .MaxBy(x => x.Title)
            );

        var sb = new StringBuilder();

        {
            var ltcY = DumpLearnsets();
            var ltcRGB = rgb.DumpLearnsets();
            var diffs = ltcY.CompareWith(ltcRGB);
            foreach (var diff in diffs)
            {
                sb.AppendLine($"No.{diff.Pokemon.Number:000} {(Monsname)diff.Pokemon.Number}");
                sb.AppendLine(diff.ToText());
            }
        }

        return sb.ToString();
    }

}
