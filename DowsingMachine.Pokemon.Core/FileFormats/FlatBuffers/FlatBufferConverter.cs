using FlatSharp;
using System.Reflection;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats.FlatBuffers;

internal class FlatBufferConverter
{
    public static T DeserializeFrom<T>(string file) where T : class
    {
        var data = File.ReadAllBytes(file);
        return DeserializeFrom<T>(data);
    }

    public static T DeserializeFrom<T>(byte[] data) where T : class
    {
        return FlatBufferSerializer.Default.Parse<T>(data);
    }

    public static Dictionary<string, string>[] TestSchema(byte[] data)
    {
        var t1 = typeof(TestSchemaByte);
        var t2 = typeof(TestSchemaUshort);
        var t4 = typeof(TestSchemaUint);
        var ts = typeof(TestSchemaString);

        var u0 = DeserializeFrom<TestSchemaUint>(data);

        var u1 = DeserializeFrom<Metatable<TestSchemaByte>>(data);
        var u2 = DeserializeFrom<Metatable<TestSchemaUshort>>(data);
        var u4 = DeserializeFrom<Metatable<TestSchemaUint>>(data);

        var names = typeof(TestSchemaByte)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.Name)
            .ToArray();

        var list = new List<Dictionary<string, string>>();
        for (var i = 0; i < u1.Tables.Length; i++)
        {
            var dict = new Dictionary<string, string>();
            for (var j = 0; j < names.Length; j++)
            {
                var v1 = (byte)t1.GetProperty(names[j]).GetValue(u1.Tables[i]);
                var v2 = (ushort)t2.GetProperty(names[j]).GetValue(u2.Tables[i]);
                var v4 = (uint)t4.GetProperty(names[j]).GetValue(u4.Tables[i]);
                var text = "";
                if (v1 == 0 && v2 == 0 && v4 == 0)
                {
                    text = "";
                }
                else
                {
                    text = $"{v1:D2}";
                    if (v2 != v1) text += $"/{v2:D4}";
                    if (v4 != v2) text += $"/{v4:D8}";
                }
                dict.Add(names[j], text);
            }
            list.Add(dict);
        }
        return list.ToArray();
    }

    public static Dictionary<string, string>[] TestSchema2(byte[] data)
    {
        var t1 = typeof(TestSchemaByte);
        var t2 = typeof(TestSchemaUshort);
        var t4 = typeof(TestSchemaUint);
        var ts = typeof(TestSchemaString);

        var u1 = DeserializeFrom<TestSchemaByte>(data);
        var u2 = DeserializeFrom<TestSchemaUshort>(data);
        var u4 = DeserializeFrom<TestSchemaUint>(data);

        var names = typeof(TestSchemaByte)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.Name)
            .ToArray();

        var list = new List<Dictionary<string, string>>();
        return list.ToArray();
    }

}
