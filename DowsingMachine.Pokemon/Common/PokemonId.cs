using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PBT.DowsingMachine.Pokemon.Common;

[JsonConverter(typeof(PokemonIdConverter))]
public struct PokemonId : IEquatable<PokemonId>, IComparable, IComparable<PokemonId>
{
    public readonly static PokemonId Empty = new();

    public static string Format = "{0:0000}.{1:000}";

    public int Number { get; set; }
    public int Form { get; set; }

    public PokemonId()
    {
        Number = 0;
        Form = 0;
    }

    public PokemonId(int number)
    {
        Number = number;
        Form = 0;
    }

    public PokemonId(int number, int form)
    {
        Number = number;
        Form = form;
    }

    public bool IsValid => Number > 0;

    public static bool operator ==(PokemonId x, PokemonId y) => x.Equals(y);

    public static bool operator !=(PokemonId x, PokemonId y) => !(x == y);

    public static bool operator ==(PokemonId x, string y) => x.Equals(Parse(y));

    public static bool operator !=(PokemonId x, string y) => !(x == y);

    public static bool operator <(PokemonId x, PokemonId y) => x.CompareTo(y) < 0;

    public static bool operator >(PokemonId x, PokemonId y) => y < x;

    public static bool operator <=(PokemonId x, PokemonId y) => x.CompareTo(y) <= 0;

    public static bool operator >=(PokemonId x, PokemonId y) => y <= x;

    public static bool operator true(PokemonId x) => x.Number > 0;

    public static bool operator false(PokemonId x) => x.Number <= 0;

    public int CompareTo(object? obj)
    {
        if (obj is PokemonId otherId)
        {
            return CompareTo(otherId);
        }
        throw new ArgumentException();
    }

    public int CompareTo(PokemonId other)
    {
        if (Number > other.Number)
        {
            return 1;
        }

        if (Number < other.Number)
        {
            return -1;
        }

        if (Form > other.Form)
        {
            return 1;
        }

        if (Form < other.Form)
        {
            return -1;
        }

        return 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is PokemonId id && Equals(id);
    }

    public bool Equals(PokemonId other)
    {
        return Number == other.Number && Form == other.Form;
    }

    public override int GetHashCode()
    {
        var value = (Number & 0xFF)
            + ((Number >> 4 & 0xFF) << 8)
            + ((Form & 0xFF) << 16)
            + ((Form >> 4 & 0xFF) << 24);
        return value;
    }

    public override string ToString()
    {
        return ToString(Format);
    }

    public string ToString(string format)
    {
        return string.Format(format, Number, Form);
    }

    public static PokemonId Parse(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Empty;
        }

        int number = 0, form = 0;

        var ms = Regex.Matches(key, @"\d+");
        if (ms.Count >= 1)
        {
            number = int.Parse(ms[0].Value);
        }
        if (ms.Count >= 2)
        {
            form = int.Parse(ms[1].Value);
        }

        return new PokemonId(number, form);
    }

    private class PokemonIdConverter : JsonConverter<PokemonId>
    {
        public override PokemonId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, PokemonId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
