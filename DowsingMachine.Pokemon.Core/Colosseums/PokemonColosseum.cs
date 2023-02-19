using PBT.DowsingMachine.Data;
using PBT.DowsingMachine.FileFormats;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core.Colosseums;

// https://github.com/PekanMmd/Pokemon-XD-Code/blob/3c2ce966e188910e118cb84c0924bd7239c5fdeb/Objects/file%20formats/XGRelocationTable.swift
// https://github.com/PekanMmd/Pokemon-XD-Code/blob/21ff17c676271c49630387531fb180ae2d99b03a/CMRelIndexes.swift
// https://www.pokecommunity.com/showthread.php?t=351350
public class PokemonColosseum : FolderProject, IPokemonProject
{
    public record MewMovePreset(int Value1, int Value2, int Move1, int Move2, int Move3, int Move4);
    public record MewMoveItem(int Move, int Value1, int Value2);
    public record TMHMData(int Type, int Move);
    public record TutorData(int Move);

    [Option]
    public GameTitle Title { get; set; }

    public GameInfo Game { get; set; }


    protected class FsysReader : DataReaderBase<byte[]>, IDataReader<string, byte[]>
    {
        public string ItemName;

        public FsysReader(string item)
        {
            ItemName = item;
        }

        public byte[] Read(string filepath)
        {
            var fsys = new FSYS(filepath);
            return fsys[ItemName];
        }
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
                Resources.Add(new DataResource("pokemon_personals")
                {
                    Reference = new FileRef(@"common.fsys"),
                    Reader = new FsysReader("common_rel")
                        .Then(data => new MatrixPack(data, 0x11C, 0x019D, 0xA5F2C).Entries)
                        .Then(data => data.Select(x => MarshalUtil.DeserializeBigEndian<PersonalColosseum>(x)).ToArray())
                });
                Resources.Add(new DataResource("tmhm_move_data")
                {
                    Reference = new FileRef(@"boot.dol"),
                    Reader = new FileReader()
                        .Then(br => br.ReadByteMatrix(8, 58, 0x351758))
                        .Then(ParseEnumerable(y => new TMHMData(y[0], y[6] << 8 | y[7])))
                });
                break;
            case GameTitle.XD:
                Resources.Add(new DataResource("boot.dol")
                {
                    Reference = new FileRef(@"boot.dol"),
                    Reader = new FileReader()
                });

                Resources.Add(new DataResource("pokemon_personals")
                {
                    Reference = new FileRef(@"common.fsys"),
                    Reader = new FsysReader("common_rel")
                        .Then(data => new MatrixPack(data, 0x124, 0x019F, 0x29DA8).Entries)
                        .Then(data => data.Select(x => MarshalUtil.DeserializeBigEndian<PersonalXD>(x)).ToArray())
                });
                Resources.Add(new DataResource("tutor_move_data")
                {
                    Reference = new FileRef(@"common.fsys"),
                    Reader = new FsysReader("common_rel")
                        .Then(data => new MatrixPack(data, 12, 12, 0xA7918).Entries)
                        .Then(data => data.Select(x => new TutorData(x[0] << 8 | x[1])).ToArray())
                });

                Resources.Add(new DataResource("tmhm_move_data")
                {
                    Reference = new ResRef(@"boot.dol"),
                    Reader = new DataReader<BinaryReader>()
                        .Then(br => br.ReadByteMatrix(8, 58, 0x4023A0))
                        .Then(ParseEnumerable(y => new TMHMData(y[0], y[6] << 8 | y[7])))
                });
                Resources.Add(new DataResource("mew_preset")
                {
                    Reference = new ResRef(@"boot.dol"),
                    Reader = new DataReader<BinaryReader>()
                        .Then(br => br.ReadByteMatrix(12, 39, 0x4198fc))
                        .Then(ParseEnumerable(ReadMewPreset))
                });
                Resources.Add(new DataResource("mew_tutor")
                {
                    Reference = new ResRef(@"boot.dol"),
                    Reader = new DataReader<BinaryReader>()
                        .Then(br => br.ReadByteMatrix(4, 94, 0x419780))
                        .Then(ParseEnumerable(y => new MewMoveItem(y[0] << 8 | y[1], y[2], y[3])))
                });
                Resources.Add(new DataResource("tutor_move_order")
                {
                    Reference = new ResRef(@"boot.dol"),
                    Reader = new DataReader<BinaryReader>()
                        .Then(br => br.ReadByteMatrix(1, 23, 0x2E7424))
                        .Then(ParseEnumerable(y => (int)y[0]))
                });

                Resources.Add(new DataResource("DeckData_Story_Pokemon")
                {
                    Reference = new FileRef(@"deck_archive.fsys"),
                    Reader = new FsysReader("DeckData_Story.bin")
                        .Then(data => new DECK(data).Read("DPKM"))
                        .Then(data => data.Select(MarshalUtil.DeserializeBigEndian<DPKM>).ToArray())
                });
                Resources.Add(new DataResource("DeckData_DarkPokemon")
                {
                    Reference = new FileRef(@"deck_archive.fsys"),
                    Reader = new FsysReader("DeckData_DarkPokemon.bin")
                        .Then(data => new DECK(data).Read("DDPK"))
                        .Then(data => data.Select(MarshalUtil.DeserializeBigEndian<DDPK>).ToArray())
                });
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
    public (DDPK, DPKM)[] GetShadowPokemon()
    {
        var ddpks = GetData<DDPK[]>("DeckData_DarkPokemon");
        var dpkms = GetData<DPKM[]>("DeckData_Story_Pokemon");
        return ddpks.Select(ddpk => {
            var dpkm = dpkms[ddpk.Index];
            return (ddpk, dpkm);
        }).ToArray();
    }


    [Data("learnsets/")]
    public LearnsetTableCollection DumpLearnsets()
    {
        var personals = Game.Title switch
        {
            GameTitle.Colosseum => GetData<PersonalColosseum[]>("pokemon_personals").Select(x => new { x.NationalDexNumber, x.LevelMoves, x.EggMoves, x.Machines, Tutors = Array.Empty<byte>() }).ToArray(),
            GameTitle.XD => GetData<PersonalXD[]>("pokemon_personals").Select(x => new { x.NationalDexNumber, x.LevelMoves, x.EggMoves, x.Machines, x.Tutors }).ToArray(),
        };
        var dexNumbers = personals
            .Select(x => (int)x.NationalDexNumber)
            .Select(x => x switch
            {
                386 => new PokemonId(386, 0),
                387 => new PokemonId(438, 0),
                388 => new PokemonId(446, 0),
                var i => new PokemonId(i, 0),
            })
            .ToArray();

        var collection = new LearnsetTableCollection("{0:000}.{1:00}");

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
            collection.Add("levelup", lt);
        }

        {
            var lt = new LearnsetTable();
            var tmlist = GetData<TMHMData[]>("tmhm_move_data");

            for (var i = 0; i < personals.Length; i++)
            {
                var data = PokemonUtils.MatchFlags(
                    tmlist,
                    personals[i].Machines.Select(x => x == 1).ToArray(),
                    (x, j) => x.Type == 0 ? $"{x.Move}:TM{j + 1:00}" : $"{x.Move}:HM{j - 49:00}");
                lt.Add(dexNumbers[i], data);
            }
            collection.Add("tm", lt);
        }

        {
            var lt = new LearnsetTable();
            for (var i = 0; i < personals.Length; i++)
            {
                var data = personals[i].EggMoves.TakeWhile(x => x > 0).Select(x => (int)x).ToArray();
                lt.Add(dexNumbers[i], data);
            }
            collection.Add("egg", lt);
        }

        if (Game.Title == GameTitle.XD)
        {
            {
                var lt = new LearnsetTable();
                var order = GetData<int[]>("tutor_move_order");
                var tmlist = GetData<TutorData[]>("tutor_move_data");
                var tmlist2 = Enumerable.Range(0, 12).Select((x, i) => tmlist[order[i]].Move).ToArray();

                for (var i = 0; i < personals.Length; i++)
                {
                    var data = PokemonUtils.MatchFlags(
                        tmlist2,
                        personals[i].Tutors.Select(x => x == 1).ToArray(),
                        (x, j) => $"{x}");
                    lt.Add(dexNumbers[i], data);
                }
                collection.Add("tutor", lt);
            }

            {
                var preset = GetData<MewMovePreset[]>("mew_preset");
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
                collection.Add("mewpattern", lt);
            }

            {
                var tutor = GetData<MewMoveItem[]>("mew_tutor");
                var lt = new LearnsetTable();
                var data = tutor.Select(x => x.Move).ToArray();
                lt.Add(new PokemonId(151), data);
                collection.Add("mewtutor", lt);
            }

            {
                var shadows = GetData(GetShadowPokemon);
                var lt = new LearnsetTable();
                foreach (var (ddpk, dpkm) in shadows)
                {
                    var data = new int[] { dpkm.Move1, dpkm.Move2, dpkm.Move3, dpkm.Move4 };
                    var id = dexNumbers[dpkm.Number];
                    lt.Add(id, data);
                }
                collection.Add("purification", lt);
            }

        }

        return collection;
    }
}
