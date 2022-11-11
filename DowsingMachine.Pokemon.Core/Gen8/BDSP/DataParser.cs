using System.Text.Json.Nodes;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public static class DataParser
{
    public static string AddPersonal(JsonNode node)
    {
        return node["monsno"].ToString().PadLeft(4, '0') + '.' + node["formno"].ToString().PadLeft(2, '0');
    }

}
