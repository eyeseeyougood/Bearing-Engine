using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Bearing;

public class ComponentConverter : JsonConverter<Component>
{
    public override Component? ReadJson(JsonReader reader, Type objectType, Component? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var typeString = jo["type"]?.ToString();

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
            NullValueHandling = serializer.NullValueHandling
        };

        return (Component?)jo.ToObject(Type.GetType(typeString), tempSerializer);
    }

    public override void WriteJson(JsonWriter writer, Component value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("type");
        writer.WriteValue(value.GetType().Name);

        var props = value.GetType().GetProperties();
        foreach (var prop in props)
        {
            if (prop.Name == "gameObject")
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