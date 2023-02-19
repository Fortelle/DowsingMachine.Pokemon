using PBT.DowsingMachine.Projects;
using System.Windows.Forms;

namespace PBT.DowsingMachine.Pokemon.Common;

public interface IPokemonProject : IMethodAuthorizable
{
    public GameInfo Game { get; set; }

    public void Set(GameTitle title)
    {
        Game = GameInfo.GameList.FirstOrDefault(x => x.Title == title);
    }

    bool IMethodAuthorizable.AuthorizeMethod(Attribute[] attributes)
    {
        var attr = attributes.OfType<TitleAttribute>().FirstOrDefault();
        if (attr is not null)
        {
            return attr.Titles.Contains(Game.Title);
        }

        return true;
    }

}
