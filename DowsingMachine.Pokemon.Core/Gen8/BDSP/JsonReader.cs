using PBT.DowsingMachine.Projects;
using System.Text.Json.Nodes;

namespace PBT.DowsingMachine.Pokemon.Core.Gen8;

public class JsonReader : DataReaderBase<JsonNode>, IDataReader<string, JsonNode>
{
    public string NodeName;

    public JsonReader()
    {
    }

    public JsonReader(string nodename)
    {
        NodeName = nodename;
    }

    public JsonNode Read(string filename)
    {
        var text = File.ReadAllText(filename);
        var json = JsonNode.Parse(text);
        if (string.IsNullOrEmpty(NodeName))
        {
            return json;
        }
        else
        {
            return json[NodeName];
        }
    }

}
