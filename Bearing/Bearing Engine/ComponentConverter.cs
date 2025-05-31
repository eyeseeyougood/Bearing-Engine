using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Bearing;

public class ComponentConverter : JsonConverter<Component>
{
    private static readonly Dictionary<string, Type> TypeMap = new()
    {
        { "MeshRenderer", typeof(MeshRenderer) },
        { "BearingRigidbody", typeof(BearingRigidbody) },
    };

    public override Component? ReadJson(JsonReader reader, Type objectType, Component? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var typeString = jo["type"]?.ToString();

        if (typeString == null || !TypeMap.TryGetValue(typeString, out var targetType))
            throw new JsonSerializationException($"Unknown component type: {typeString}");

        var tempSerializer = new JsonSerializer
        {
            ContractResolver = serializer.ContractResolver,
            NullValueHandling = serializer.NullValueHandling
        };

        return (Component?)jo.ToObject(targetType, tempSerializer);
    }

    public override void WriteJson(JsonWriter writer, Component value, JsonSerializer serializer)
    {
        var jo = JObject.FromObject(value, serializer);
        jo.AddFirst(new JProperty("type", value.GetType().Name));
        jo.WriteTo(writer);
    }
}