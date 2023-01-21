using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.FileFormats;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core.Colosseums;

// https://github.com/PekanMmd/Pokemon-XD-Code/blob/3c2ce966e188910e118cb84c0924bd7239c5fdeb/Objects/file%20formats/XGRelocationTable.swift
// https://github.com/PekanMmd/Pokemon-XD-Code/blob/21ff17c676271c49630387531fb180ae2d99b03a/CMRelIndexes.swift
// https://www.pokecommunity.com/showthread.php?t=351350
public class PokemonColosseum : DataProject, IPokemonProject
{
    public record MewMovePreset(int Value1, int Value2, int Move1, int Move2, int Move3, int Move4);
    public record MewMoveItem(int Move, int Value1, int Value2);
    public record TMHMData(int Type, int Move);
    public record TutorData(int Move);

    [Option]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }


    protected class FsysReader : DataReader<FSYS, byte[]>
    {
        public string ItemName;

        public FsysReader(string path, string item) : base(path)
        {
            ItemName = item;
        }

        protected override FSYS Open() => new FSYS(Project.As<IFolderProject>().GetPath(RelatedPath));
        protected override byte[] Read(FSYS fsys) => fsys[ItemName];
    }

    public PokemonColosseum()
    {

    }

    public override void Configure()
    {
        base.Configure();

        ((IPokemonProject)this).Set(Title);

        switch (Title)
        {
            case GameTitle.Colosseum:
                AddReference("PersonalTable",
                     new FsysReader("common.fsys", "common_rel"),
                     data => new MatrixPack(data, 0x11C, 0x019D, 0xA5F2C).Entries,
                     data => data.Select(x => MarshalUtil.DeserializeBigEndian<PersonalColosseum>(x)).ToArray()
                     );
                AddReference("TMHMData",
                    new MatrixReader("boot.dol", 8, 58, 0x351758),
                    x => ParseEnumerable(x, y => new TMHMData(y[0], y[6] << 8 | y[7]))
                    );
                break;
            case GameTitle.XD:
                AddReference("PersonalTable",
                     new FsysReader("common.fsys", "common_rel"),
                     data => new MatrixPack(data, 0x124, 0x019F, 0x29DA8).Entries,
                     data => data.Select(x => MarshalUtil.DeserializeBigEndian<PersonalXD>(x)).ToArray()
                     );
                AddReference("TMHMData",
                    new MatrixReader("boot.dol", 8, 58, 0x4023A0),
                    x => ParseEnumerable(x, y => new TMHMData(y[0], y[6] << 8 | y[7]))
                    );

                AddReference("TutorData",
                     new FsysReader("common.fsys", "common_rel"),
                     data => new MatrixPack(data, 12, 12, 0xA7918).Entries,
                     data => data.Select(x => new TutorData(x[0] << 8 | x[1])).ToArray()
                     );

                AddReference("MewPreset",
                    new MatrixReader("boot.dol", 12, 39, 0x4198fc),
                    x => ParseEnumerable(x, ReadMewPreset)
                    );

                AddReference("MewTutor",
                    new MatrixReader("boot.dol", 4, 94, 0x419780),
                    x => ParseEnumerable(x, y => new MewMoveItem(y[0] << 8 | y[1], y[2], y[3]))
                    );

                AddReference("TutorOrder",
                    new MatrixReader("boot.dol", 1, 23, 0x2E7424),
                    x => ParseEnumerable(x, y => y[0])
                    );

                AddReference("DeckData_Story_Pokemon",
                     new FsysReader("deck_archive.fsys", "DeckData_Story.bin"),
                     data => new DECK(data).Read("DPKM").ToArray(),
                     data => data.Select(x => new TutorData(x[0] << 8 | x[1])).ToArray()
                     );

                AddReference("DeckData_DarkPokemon_Index",
                     new FsysReader("deck_archive.fsys", "DeckData_DarkPokemon.bin"),
                     data => new DECK(data).Read("DDPK").ToArray(),
                     data => data.Select(x => x[6] << 8 | x[7]).ToArray()
                     );
                break;

        }

    }

    public MewMovePreset ReadMewPreset(byte[] data)
    {
        return new MewMovePreset(
            data[0] << 8 | data[1],
            data[2] << 8 | data[3],
            data[4] << 8 | data[5],
            data[6] << 8 | data[7],
            data[8] << 8 | data[9],
            data[10] << 8 | data[11]
        );
    }

    [Test]
    public string[][] Text()
    {
        var indexes = GetData<int[]>("DeckData_DarkPokemon_Index");
        var pokemon = GetData<byte[][]>("DeckData_Story_Pokemon");
        var pokemon2 = indexes.Select(i => pokemon[i]).ToArray();
        var list = new List<string[]>();
        foreach (var pdata in pokemon2)
        {
            var pi = pdata[0] << 8 | pdata[1];
            var data = new[]
            {
                    pdata[0x14] << 8 | pdata[0x15],
                    pdata[0x16] << 8 | pdata[0x17],
                    pdata[0x18] << 8 | pdata[0x19],
                    pdata[0x1A] << 8 | pdata[0x1B],
            };
        }
        return list.ToArray();
    }


    [Test]
    public PokemonId[] GetPokemonIds(int[] dex)
    {
        var ids = dex
            .Select(x => x switch
            {
                386 => new PokemonId(386, 0),
                387 => new PokemonId(438, 0),
                388 => new PokemonId(446, 0),
                var i => new PokemonId(i, 0),
            })
            .ToArray();
        return ids;
    }


    [Extraction]
    public IEnumerable<string> ExtractLearnset()
    {
        var suffix = Game.Title switch
        {
            GameTitle.Colosseum => "colosseum",
            GameTitle.XD => "galeofdarkness",
        };
        var format = "{0:000}.{1:00}";
        var personals = Game.Title switch
        {
            GameTitle.Colosseum => GetData<PersonalColosseum[]>("PersonalTable").Select(x => new { x.NationalDexNumber, x.LevelMoves, x.EggMoves, x.Machines }).ToArray(),
            GameTitle.XD => GetData<PersonalXD[]>("PersonalTable").Select(x => new { x.NationalDexNumber, x.LevelMoves, x.EggMoves, x.Machines }).ToArray(),
        };
        var dexNumbers = GetPokemonIds(personals.Select(x => (int)x.NationalDexNumber).ToArray());

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < personals.Length; i++)
            {
                var data = personals[i].LevelMoves
                    .TakeWhile(x => x.Move > 0)
                    .Select(x => $"{x.Move}:{x.Level}")
                    .ToArray();
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.levelup.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            var tmlist = GetData<TMHMData[]>("TMHMData");

            for (var i = 0; i < personals.Length; i++)
            {
                var data = PokemonUtils.MatchFlags(
                    tmlist,
                    personals[i].Machines.Take(58).Select(x => x == 1).ToArray(),
                    (x, j) => x.Type == 0 ? $"{x.Move}:TM{j + 1:00}" : $"{x.Move}:HM{j - 49:00}");
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.tm.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < personals.Length; i++)
            {
                var data = personals[i].EggMoves.TakeWhile(x => x > 0).Select(x => (int)x).ToArray();
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.egg.txt");
            lt.Save(path, format);
            yield return path;
        }


        if (Game.Title != GameTitle.XD) yield break;

        {
            var lt = new LearnsetTable();
            var order = GetData<int[]>("TutorOrder");
            var tmlist = GetData<TutorData[]>("TutorData");
            var tmlist2 = Enumerable.Range(0, 12).Select((x, i) => tmlist[order[i]].Move).ToArray();

            for (var i = 0; i < personals.Length; i++)
            {
                var data = PokemonUtils.MatchFlags(
                    tmlist2,
                    personals[i].Machines.Skip(58).Select(x => x == 1).ToArray(),
                    (x, j) => $"{x}");
                lt.Add(dexNumbers[i], data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.tutor.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var preset = GetData<MewMovePreset[]>("MewPreset");
            var lt = new LearnsetTable();
            for (var i = 0; i < preset.Length; i++)
            {
                var data = new[] {
                    preset[i].Move1,
                    preset[i].Move2,
                    preset[i].Move3,
                    preset[i].Move4
                }.Select(x => x > 999 ? 0 : x).ToArray();
                lt.Add(new PokemonId(151), data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.mewpattern.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var tutor = GetData<MewMoveItem[]>("MewTutor");
            var lt = new LearnsetTable();
            var data = tutor.Select(x => x.Move).ToArray();
            lt.Add(new PokemonId(151), data);
            var path = Path.Combine(OutputFolder, $"{suffix}.mewtutor.txt");
            lt.Save(path, format);
            yield return path;
        }

        {
            var indexes = GetData<int[]>("DeckData_DarkPokemon_Index");
            var pokemon = GetData<byte[][]>("DeckData_Story_Pokemon");
            var pokemon2 = indexes.Select(i => pokemon[i]).ToArray();
            var lt = new LearnsetTable();
            foreach (var pdata in pokemon2)
            {
                var pi = pdata[0] << 8 | pdata[1];
                var data = new[]
                {
                    pdata[0x14] << 8 | pdata[0x15],
                    pdata[0x16] << 8 | pdata[0x17],
                    pdata[0x18] << 8 | pdata[0x19],
                    pdata[0x1A] << 8 | pdata[0x1B],
                };
                lt.Add(new PokemonId(pi), data);
            }
            var path = Path.Combine(OutputFolder, $"{suffix}.purification.txt");
            lt.Save(path, format);
            yield return path;
        }

    }
}
