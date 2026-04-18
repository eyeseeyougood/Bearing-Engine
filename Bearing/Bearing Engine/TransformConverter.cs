using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Bearing;

public class TransformConverter : JsonConverter<Transform>
{
    public override Transform? ReadJson(JsonReader reader, Type objectType, Transform? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var typeString = jo["transformType"]?.ToString();

        if (typeString == null)
            throw new JsonSerializationException($"Unknown component type: {typeString}");

        if (Type.GetType(typeString) == null)
            if (Type.GetType("Bearing." + typeString) == null)
                throw new JsonSerializationException($"Unknown component type: {typeString}");
            else
                typeString = "Bearing." + typeString;

        if (Type.GetType(typeString) == null)
            throw new JsonSerializationException($"Unknown component type: {typeString}");

        var tempSerializer = new JsonSerializer
        {
            ContractResolver = serializer.ContractResolver,
            NullValueHandling = serializer.NullValueHandling,
            Converters = {
                new ColliderConverter(),
            }
        };

        return (Transform?)jo.ToObject(Type.GetType(typeString), tempSerializer);
    }

    public override void WriteJson(JsonWriter writer, Transform value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("transformType");
        writer.WriteValue(value.GetType().Name);

        var props = value.GetType().GetProperties();
        foreach (var prop in props)
        {
            string[] ignores = ["gameObject", "parent"];
            if (ignores.Contains(prop.Name))
                continue;

            if (prop.CanRead && prop.GetIndexParameters().Length == 0)
            {
                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, prop.GetValue(value));
            }
        }

        writer.WriteEndObject();
    }
}