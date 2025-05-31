using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Bearing;

public class ParameterConverter : JsonConverter<Dictionary<string, object>>
{
    public override Dictionary<string, object> ReadJson(JsonReader reader, Type objectType, Dictionary<string, object>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var raw = JObject.Load(reader);
        var result = new Dictionary<string, object>();

        foreach (var pair in raw)
        {
            if (pair.Value is not JObject paramObj)
                throw new JsonSerializationException($"Expected an object for parameter '{pair.Key}', but got: {pair.Value.Type}");

            var typeStr = paramObj["type"]?.ToString();
            if (typeStr == null)
                throw new JsonSerializationException($"Missing 'type' field in parameter '{pair.Key}'.");

            object? value = typeStr switch
            {
                "int" => paramObj["value"]?.ToObject<int>(),
                "float" => paramObj["value"]?.ToObject<float>(),
                "string" => paramObj["value"]?.ToObject<string>(),
                "Vector2" => paramObj.ToObject<Vector2>(serializer),
                "Vector3" => paramObj.ToObject<Vector3>(serializer),
                "Vector4" => paramObj.ToObject<Vector4>(serializer),
                _ => throw new JsonSerializationException($"Unknown type '{typeStr}' for parameter '{pair.Key}'.")
            };

            result[pair.Key] = value!;
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, object>? value, JsonSerializer serializer)
    {
        var jo = new JObject();
        if (value != null)
        {
            foreach (var pair in value)
            {
                var inner = JObject.FromObject(pair.Value!, serializer);
                inner.AddFirst(new JProperty("type", pair.Value?.GetType().Name ?? "Unknown"));
                jo.Add(pair.Key, inner);
            }
        }

        jo.WriteTo(writer);
    }
}

