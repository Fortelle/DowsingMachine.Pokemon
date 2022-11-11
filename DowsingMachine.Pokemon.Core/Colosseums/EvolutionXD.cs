namespace PBT.DowsingMachine.Pokemon.Core.Projects.Colosseums;

public struct EvolutionXD
{
    public byte Method;
    public byte _;
    public ushort Value;
    public ushort Target;

    public override string ToString()
    {
        return $"{Method},{Value},{Target}";
    }
}
