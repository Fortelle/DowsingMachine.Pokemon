using PBT.DowsingMachine.FileFormats;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core;
using PBT.DowsingMachine.Pokemon.Core.FileFormats;
using PBT.DowsingMachine.Pokemon.Core.Gen3;
using PBT.DowsingMachine.Pokemon.Core.Gen4;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Games;

public class PokemonProjectIV : PokemonProjectDS
{
    public int InternalPokemonCount = 501;

    public PokemonProjectIV(GameTitle title, string baseFolder, Dictionary<string, string[]> langs) : base(title, baseFolder, langs)
    {
        switch (title)
        {
            case GameTitle.Diamond:
            case GameTitle.Pearl:
                AddReference("PersonalTable",
                    new NarcReader(@"root\poketool\personal\personal.narc"),
                    MarshalArray<Personal4>
                    );
                AddReference("Wazaoboe",
                    new NarcReader(@"root\poketool\personal\wotbl.narc"),
                    narc => ParseArray(narc, ReadLevelupMoves)
                    );
                AddReference("MachineList",
                    new StreamBinaryReader(@"root\ftc\arm9.bin", 0xFA318),
                    ReadTMHMList
                    );
                AddReference("EggMoves",
                    new OverlayReader(@"root\ftc\overlay9_5", 0x21648),
                    PokemonProjectIII.ReadEggMoves
                    );
                AddReference("msg",
                    new NarcReader(@"root\msgdata\msg.narc"),
                    ReadMessage
                    );
                break;
            case GameTitle.Platinum:
                AddReference("PersonalTable",
                    new NarcReader(@"root\poketool\personal\pl_personal.narc"),
                    MarshalArray<Personal4>
                    );
                AddReference("Wazaoboe",
                    new NarcReader(@"root\poketool\personal\wotbl.narc"),
                    narc => ParseEnumerable(narc, ReadLevelupMoves)
                    );
                AddReference("MachineList",
                    new StreamBinaryReader(@"root\ftc\arm9.bin", 0xF028C),
                    ReadTMHMList
                    );
                AddReference("EggMoves",
                    new OverlayReader(@"root\ftc\overlay9_5", 0x29012),
                    PokemonProjectIII.ReadEggMoves
                    );
                AddReference("WazaOshieTable",
                    new OverlayReader(@"root\ftc\overlay9_5", 0x2FD54),
                    br => MatrixPack.From(br, 12, 38).Marshal<PlatinumWazaOshieList>()
                    );
                AddReference("TutorMoves",
                    new OverlayReader(@"root\ftc\overlay9_5", 0x2FF1C),
                    ReadTutorMoveFlags
                    );

                AddReference("msg",
                    new NarcReader(@"root\msgdata\msg.narc"),
                    ReadMessage
                    );

                break;
            case GameTitle.HeartGold:
            case GameTitle.SoulSilver:
                AddReference("PersonalTable",
                    new NarcReader(@"root\a\0\0\2"),
                    MarshalArray<Personal4>
                    );
                AddReference("Wazaoboe",
                    new NarcReader(@"root\a\0\3\3"),
                    narc => ParseEnumerable(narc, ReadLevelupMoves)
                    );
                AddReference("EggMoves",
                    new NarcReader(@"root\data\kowaza.narc"),
                    narc => ParseEnumerable(narc, PokemonProjectIII.ReadEggMoves)
                    );
                AddReference("WazaOshieTable",
                    new OverlayReader(@"root\ftc\overlay9_1", 0x00023954),
                    br => new MatrixPack(br, 4, 52).Entries,
                    MarshalArray<HGSSTutorMove>
                    );
                AddReference("TutorMoves",
                    new MatrixReader(@"root\fielddata\wazaoshie\waza_oshie.bin", 8),
                    narc => ParseEnumerable(narc, ReadTutorMoveFlagsHGSS)
                    );
                AddReference("MachineList",
                    new StreamBinaryReader(@"root\ftc\arm9_decompressed.bin", 0x000FF84C),
                    ReadTMHMList
                    );

                AddReference("msg",
                    new NarcReader(@"root\a\0\2\7"),
                    ReadMessage
                    );

                break;
        }
    }

    public static LevelupMove[] ReadLevelupMoves(BinaryReader br)
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

    public int[] ReadTMHMList(BinaryReader br)
    {
        return Enumerable.Range(1, 100).Select(_ => (int)br.ReadInt16()).ToArray();
    }

    public static bool[][] ReadTutorMoveFlags(BinaryReader br)
    {
        var data = Enumerable.Range(0, 505)
            .Select(_ => br.ReadBytes(5))
            .Select(x => PokemonUtils.ToBooleans(x))
            .ToArray();
        return data;
    }

    public static bool[] ReadTutorMoveFlagsHGSS(BinaryReader br)
    {
        var data = br.ReadBytes(8)
            .SelectMany(x => PokemonUtils.ToBooleans(x))
            .ToArray();
        return data;
    }

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

    [Dump]
    public IEnumerable<string> DumpLearnset()
    {
        var suffix = Game.Title switch {
            GameTitle.Diamond or GameTitle.Pearl => "diamondpearl",
            GameTitle.Platinum => "platinum",
            GameTitle.HeartGold or GameTitle.SoulSilver => "heartgoldsoulsilver",
        };
        var format = "{0:000}.{1:00}";

        var personals = GetData<Personal4[]>("PersonalTable");
        var dexNumbers = GetPokemonIds(personals.Length);

        {
            var lt = new LearnsetTable();
            var tmlist = GetData<int[]>("MachineList");

            for (var i = 0; i < personals.Length; i++)
            {
                var tm = PokemonUtils.ToBooleans(personals[i].Machine1, personals[i].Machine2, personals[i].Machine3, personals[i].Machine4);
                var data = PokemonUtils.MatchFlags(tmlist, tm, (x, j) => j < 92 ? $"{x}:TM{j + 1:00}" : $"{x}:HM{j - 91:00}");
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tm.txt");
            lt.Save(path, format);
            yield return path;

            var path2 = Path.Combine(OutputPath, $"{suffix}.tm.json");
            lt.SaveJson(path2, true, true);
            yield return path2;
        }

        {
            var lt = new LearnsetTable();
            var moves = GetData<LevelupMove[][]>("Wazaoboe");
            for (var i = 0; i < personals.Length; i++)
            {
                var data = moves[i].Select(x => $"{x.Move}:{x.Level}").ToArray();
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputPath, $"{suffix}.levelup.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();

            // fielddata/script/r228r0201.ev => ev_r228r0201_oldman1
            lt.Add(new PokemonId(6), 307);
            lt.Add(new PokemonId(157), 307);
            lt.Add(new PokemonId(257), 307);
            lt.Add(new PokemonId(392), 307);

            lt.Add(new PokemonId(9), 308);
            lt.Add(new PokemonId(160), 308);
            lt.Add(new PokemonId(260), 308);
            lt.Add(new PokemonId(395), 308);

            lt.Add(new PokemonId(3), 338);
            lt.Add(new PokemonId(154), 338);
            lt.Add(new PokemonId(254), 338);
            lt.Add(new PokemonId(389), 338);

            // fielddata/script/r210br0101.ev => ev_r210br0101_dragon
            var DRAGON_TYPE = 16;
            for (var i = 0; i < personals.Length; i++)
            {
                if(personals[i].Type1 == DRAGON_TYPE || personals[i].Type2 == DRAGON_TYPE)
                {
                    lt.Add(dexNumbers[i], 434);
                }
            }
            lt.Add(new PokemonId(493, DRAGON_TYPE), 434);

            var path = Path.Combine(OutputPath, $"{suffix}.tutor_ult.txt");
            lt.Save(path, format);
            yield return path;
        }

        if (Game.Title is GameTitle.Platinum or GameTitle.HeartGold or GameTitle.SoulSilver)
        {
            var lt = new LearnsetTable();
            var flags = GetData<bool[][]>("TutorMoves");
            var tmlist = Game.Title == GameTitle.HeartGold || Game.Title == GameTitle.SoulSilver
                ? GetData<HGSSTutorMove[]>("WazaOshieTable").Select(x => (int)x.Waza).ToArray()
                : GetData<PlatinumWazaOshieList[]>("WazaOshieTable").Select(x => (int)x.Waza).ToArray();
            var number = GetWazaOshieIds(flags.Length);

            for (var i = 0; i < flags.Length; i++)
            {
                var data = PokemonUtils.MatchFlags(tmlist, flags[i]);
                lt.Add(number[i], data);
            }
            var path = Path.Combine(OutputPath, $"{suffix}.tutor.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            var eggs = Game.Title == GameTitle.HeartGold || Game.Title == GameTitle.SoulSilver
                ? GetData<Dictionary<int, int[]>[]>("EggMoves")[0]
                : GetData<Dictionary<int, int[]>>("EggMoves");
            foreach (var (index, moves) in eggs)
            {
                lt.Add(dexNumbers[index], moves);
            }
            //lt.Append(new PokemonId(175), 344);

            var path = Path.Combine(OutputPath, $"{suffix}.egg.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            // hardcoded moves
            var lt = new LearnsetTable();
            // field/sodateya.c => PichuExtraCheck
            lt.Add(new PokemonId(175), 344);
            if (Game.Title >= GameTitle.Platinum)
            {
                // rotom: https://github.com/pret/pokeheartgold/blob/cec81057d49fd9f95515166ee92311a49c02d564/src/pokemon.c#L3627
                var rotom_form_moves = new[] { 0, 315, 56, 59, 403, 437 };
                for (var i = 1; i < 6; i++)
                {
                    lt.Add(new PokemonId(479, i), rotom_form_moves[i]);
                }
            }

            var path = Path.Combine(OutputPath, $"{suffix}.special.txt");
            lt.Save(path, format);
            yield return path;
        }

    }
}
