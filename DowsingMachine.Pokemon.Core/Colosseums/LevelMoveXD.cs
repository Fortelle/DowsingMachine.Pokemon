using PBT.DowsingMachine.Pokemon.Common;

namespace PBT.DowsingMachine.Pokemon.Core.Colosseums;

public struct LevelMoveXD
{
    public byte Level;
    public byte _;
    public ushort Move;

    public LevelupMove ToLevelupMove()
    {
        return new LevelupMove(Move, Level);
    }

    public override string ToString()
    {
        return $"{Move}:{Level}";
    }
}
