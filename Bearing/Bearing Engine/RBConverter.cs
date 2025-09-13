using BulletSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace Bearing;
public class RBConverter : JsonConverter<BearingRigidbody>
{
    public override void WriteJson(JsonWriter writer, BearingRigidbody value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteValue("BearingRigidbody");
        writer.WritePropertyName("Collider");
        serializer.Serialize(writer, value.Collider);
        writer.WriteEndObject();
    }

    public override BearingRigidbody ReadJson(JsonReader reader, Type objectType, BearingRigidbody existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = (JObject)JObject.ReadFrom(reader);

        BearingRigidbody rb = new BearingRigidbody();

        JsonReader r = new JsonTextReader(new StringReader(jo["Collider"].ToString()));

        rb.UpdateCollider((CollisionShape)serializer.Deserialize(r));

        r.Close();

        return rb;
    }
}