namespace PBT.DowsingMachine.Pokemon.Common;

public class GameInfo
{
    public GameTitle Title { get; set; }
    public Generation Generation { get; set; }
    public Platform Platform { get; set; }
    public int PokemonCount { get; set; }

    public GameInfo(GameTitle title, Generation generation, Platform platform)
    {
        Title = title;
        Generation = generation;
        Platform = platform;

        PokemonCount = -1;
    }

    public GameInfo(GameTitle title, Generation generation, Platform platform, int count)
    {
        Title = title;
        Generation = generation;
        Platform = platform;
        PokemonCount = count;
    }

    public static GameInfo[] GameList = new[]
    {
        new GameInfo(GameTitle.Red, Generation.I, Platform.NintendoGameBoy, 151),
        new GameInfo(GameTitle.Green, Generation.I, Platform.NintendoGameBoy, 151),
        new GameInfo(GameTitle.Blue, Generation.I, Platform.NintendoGameBoy, 151),
        new GameInfo(GameTitle.Yellow, Generation.I, Platform.NintendoGameBoyColor, 151),

        new GameInfo(GameTitle.Gold, Generation.II, Platform.NintendoGameBoyColor, 251),
        new GameInfo(GameTitle.Crystal, Generation.II, Platform.NintendoGameBoyColor, 251),

        new GameInfo(GameTitle.Ruby, Generation.III, Platform.NintendoGameBoyAdvance, 386),
        new GameInfo(GameTitle.Sapphire, Generation.III, Platform.NintendoGameBoyAdvance, 386),
        new GameInfo(GameTitle.FireRed, Generation.III, Platform.NintendoGameBoyAdvance, 386),
        new GameInfo(GameTitle.LeafGreen, Generation.III, Platform.NintendoGameBoyAdvance, 386),
        new GameInfo(GameTitle.Emerald, Generation.III, Platform.NintendoGameBoyAdvance, 386),
        new GameInfo(GameTitle.Colosseum, Generation.III, Platform.NintendoGameCube, 386),
        new GameInfo(GameTitle.XD, Generation.III, Platform.NintendoGameCube, 388),

        new GameInfo(GameTitle.Diamond, Generation.IV, Platform.NintendoDS, 493),
        new GameInfo(GameTitle.Pearl, Generation.IV, Platform.NintendoDS, 493),
        new GameInfo(GameTitle.Platinum, Generation.IV, Platform.NintendoDS, 493),
        new GameInfo(GameTitle.HeartGold, Generation.IV, Platform.NintendoDS, 493),
        new GameInfo(GameTitle.SoulSilver, Generation.IV, Platform.NintendoDS, 493),

        new GameInfo(GameTitle.Black, Generation.V, Platform.NintendoDS, 649),
        new GameInfo(GameTitle.White,Generation.V, Platform.NintendoDS, 649),
        new GameInfo(GameTitle.Black2, Generation.V, Platform.NintendoDS, 649),
        new GameInfo(GameTitle.White, Generation.V, Platform.NintendoDS, 649),

        new GameInfo(GameTitle.X, Generation.VI, Platform.Nintendo3DS, 721),
        new GameInfo(GameTitle.Y, Generation.VI, Platform.Nintendo3DS, 721),
        new GameInfo(GameTitle.OmegaRuby, Generation.VI, Platform.Nintendo3DS, 721),
        new GameInfo(GameTitle.AlphaSapphire, Generation.VI, Platform.Nintendo3DS, 721),

        new GameInfo(GameTitle.RedVC, Generation.I, Platform.Nintendo3DS, 151),
        new GameInfo(GameTitle.BlueVC, Generation.I, Platform.Nintendo3DS, 151),
        new GameInfo(GameTitle.YellowVC, Generation.I, Platform.Nintendo3DS, 151),

        new GameInfo(GameTitle.Sun, Generation.VII, Platform.Nintendo3DS, 802),
        new GameInfo(GameTitle.Moon, Generation.VII, Platform.Nintendo3DS, 802),
        new GameInfo(GameTitle.UltraSun, Generation.VII, Platform.Nintendo3DS, 807),
        new GameInfo(GameTitle.UltraMoon, Generation.VII, Platform.Nintendo3DS, 807),
        new GameInfo(GameTitle.LetsGoPikachu, Generation.VII, Platform.NintendoSwitch, 809),
        new GameInfo(GameTitle.LetsGoEevee,Generation.VII, Platform.NintendoSwitch, 809),

        new GameInfo(GameTitle.Sword, Generation.VIII, Platform.NintendoSwitch, 898),
        new GameInfo(GameTitle.Shield, Generation.VIII, Platform.NintendoSwitch, 898),
        new GameInfo(GameTitle.BrilliantDiamond, Generation.VII, Platform.NintendoSwitch, 898),
        new GameInfo(GameTitle.ShiningPearl, Generation.VII, Platform.NintendoSwitch, 898),
        new GameInfo(GameTitle.Arceus, Generation.VII, Platform.NintendoSwitch),

        new GameInfo(GameTitle.Scarlet, Generation.IX, Platform.NintendoSwitch, 1010),
        new GameInfo(GameTitle.Violet, Generation.IX, Platform.NintendoSwitch, 1010),
    };

}
