using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using BulletSharp;
using BulletSharp.Math;

namespace Bearing;

public class ColliderConverter : JsonConverter<CollisionShape>
{
    public override CollisionShape? ReadJson(JsonReader reader, Type objectType, CollisionShape? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var typeString = jo["type"]?.ToString();

        if (typeString == null)
            throw new JsonSerializationException($"Unknown component type: {typeString}");

        var tempSerializer = new JsonSerializer
        {
            ContractResolver = serializer.ContractResolver,
            NullValueHandling = serializer.NullValueHandling 
        };

        // convert to object
        CollisionShape? nShape = null;
        switch (typeString)
        {
            case "BoxShape":
                BulletSharp.Math.Vector3 vec = new();
                var v = jo["boxHalfExtents"];
                if (v.Type.ToString() == "Float")
                {
                    vec = new((float)jo["boxHalfExtents"]);
                }
                else
                {
                    vec = new(
                        (float)jo["boxHalfExtents"]["X"],
                        (float)jo["boxHalfExtents"]["Y"],
                        (float)jo["boxHalfExtents"]["Z"]
                        );
                }

                nShape = new BoxShape(vec);
                break;
            case "SphereShape":
                nShape = new SphereShape((float)jo["radius"]);
                break;
        }

        return nShape;
    }

    public override void WriteJson(JsonWriter writer, CollisionShape value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("type");
        writer.WriteValue(value.GetType().Name);

        value.LocalScaling = Vector3.One; // dont save with modified scaling
        switch (value.GetType().Name)
        {
            case "BoxShape":
                writer.WritePropertyName("boxHalfExtents");
                serializer.Serialize(writer, ((BoxShape)value).HalfExtentsWithMargin);
                break;
            case "SphereShape":
                writer.WritePropertyName("radius");
                serializer.Serialize(writer, ((SphereShape)value).Radius);
                break;
        }

        writer.WriteEndObject();
    }
}