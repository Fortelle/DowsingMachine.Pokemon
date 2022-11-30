using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.Gen8;
using PBT.DowsingMachine.Pokemon.Core.Gen9;
using PBT.DowsingMachine.Pokemon.Games;
using PBT.DowsingMachine.Projects;

namespace DowsingMachine.Pokemon.Launcher;

public static class ProjectLoader
{
    public static IEnumerable<DataProject> Load()
    {
        // ours
        var inputFolder = @"E:\Pokemon\Resources\Unpacked\"; 
        var outputFolder = @"E:\Pokemon\Output\";

        yield return new PokemonProjectVII(GameTitle.Sun, Path.Combine(inputFolder, "3DS\\s"))
        {
            OutputPath = Path.Combine(outputFolder, @"SM"),
        };

        yield return new PokemonProjectSWSH(GameTitle.Sword, "1.3.2", Path.Combine(inputFolder, "NS\\Sword1.0"), Path.Combine(inputFolder, "NS\\Sword1.3.2"))
        {
            OutputPath = Path.Combine(outputFolder, @"SWSH\1.3.2"),
        };

        yield return new PokemonProjectSV(GameTitle.Scarlet, "1.0.0", Path.Combine(inputFolder, "NS\\SV"))
        {
            OutputPath = Path.Combine(outputFolder, @"SV"),
        };

        yield return new PokemonProjectSV(GameTitle.Scarlet, "1.0.1", Path.Combine(inputFolder, "NS\\SV"), Path.Combine(inputFolder, "NS\\Scarlet1.0.1"))
        {
            OutputPath = Path.Combine(outputFolder, @"SV"),
        };
    }

}
