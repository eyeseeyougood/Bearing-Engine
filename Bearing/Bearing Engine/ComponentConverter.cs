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
        var jo = JObject.FromObject(value, serializer);
        jo.AddFirst(new JProperty("type", value.GetType().Name));
        jo.WriteTo(writer);
    }
}