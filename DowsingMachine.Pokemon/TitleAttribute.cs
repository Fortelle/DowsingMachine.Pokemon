using PBT.DowsingMachine.Pokemon.Common;

namespace PBT.DowsingMachine.Pokemon;

[AttributeUsage(AttributeTargets.Method)]
public class TitleAttribute : Attribute
{
    public GameTitle[] Titles { get; set; }

    public TitleAttribute(params GameTitle[] titles)
    {
        this.Titles = titles;
    }
}
