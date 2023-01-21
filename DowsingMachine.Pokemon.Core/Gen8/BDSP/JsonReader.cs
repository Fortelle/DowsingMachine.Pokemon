using PBT.DowsingMachine.Projects;
using System.Text.Json.Nodes;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public class JsonReader : DataReader<JsonNode>
{
    public string NodeName;
    public JsonReader(string name) : base(name)
    {
    }

    public JsonReader(string name, string nodename) : base(name)
    {
        NodeName = nodename;
    }

    protected override JsonNode Open()
    {
        var path = Project.As<IFolderProject>().GetPath(RelatedPath);
        var text = File.ReadAllText(path);
        var json = JsonNode.Parse(text);
        return json;
    }

    protected override JsonNode Read(JsonNode cache)
    {
        if (string.IsNullOrEmpty(NodeName))
        {
            return cache;
        }
        else
        {
            return cache[NodeName];
        }
    }
}

