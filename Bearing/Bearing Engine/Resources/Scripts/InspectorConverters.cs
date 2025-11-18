using Bearing;
using BulletSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public class ShaderConverter : JsonConverter<Shader>
{
    public override void WriteJson(JsonWriter writer, Shader value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("vert");
        writer.WriteValue(value.vert);
        writer.WritePropertyName("frag");
        writer.WriteValue(value.frag);
        writer.WriteEndObject();
    }

    public override Shader ReadJson(JsonReader reader, Type objectType, Shader existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = (JObject)JObject.ReadFrom(reader);

        return new Shader(jo["vert"]?.Value<string>(), jo["frag"]?.Value<string>());
    }
}






public class MeshConverter : JsonConverter<Mesh>
{
    public override void WriteJson(JsonWriter writer, Mesh value, JsonSerializer serializer)
    {
        writer.WriteValue(value.name);
    }

    public override Mesh ReadJson(JsonReader reader, Type objectType, Mesh existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = (JObject)JObject.ReadFrom(reader);

        return new Mesh3D(jo["mesh"]?.Value<string>());
    }
}




public class GameObjectConverter : JsonConverter<GameObject>
{
    public override void WriteJson(JsonWriter writer, GameObject value, JsonSerializer serializer)
    {
        if (value.tag == "EditorObject")
            return;

        writer.WriteStartObject();

        var props = value.GetType().GetProperties();
        foreach (var prop in props)
        {
            if (prop.Name == "parent")
                continue;

            if (prop.CanRead && prop.GetIndexParameters().Length == 0)
            {
                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, prop.GetValue(value));
            }
        }

        writer.WriteEndObject();
    }

    public override GameObject ReadJson(JsonReader reader, Type objectType, GameObject existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = (JObject)JObject.ReadFrom(reader);

        return jo.ToObject<GameObject>();
    }
}






public class ShaderParamConverter : JsonConverter<ShaderParam>
{
    public override void WriteJson(JsonWriter writer, ShaderParam value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WriteValue(value.name);
        writer.WritePropertyName(value.GetData().GetType().Name.ToLower());
        object data = value.GetData();
        Type t = data.GetType();
        serializer.Serialize(writer, data);
        writer.WriteEndObject();
    }

    public override ShaderParam ReadJson(JsonReader reader, Type objectType, ShaderParam existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        string v = jo["use"]?.Value<int>() switch
        {
            0 => "float",
            1 => "int",
            2 => "vector2",
            3 => "vector3",
            4 => "vector4",
            5 => "matrix4"
        };
        ShaderParam result = new ShaderParam(jo["name"]?.Value<string>(), jo[v]?.Value<object>());

        return result;
    }
}




public class Vector4Converter : JsonConverter<Vector4>
{
    public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.X);
        writer.WritePropertyName("y");
        writer.WriteValue(value.Y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.Z);
        writer.WritePropertyName("w");
        writer.WriteValue(value.W);
        writer.WriteEndObject();
    }

    public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Vector4(
            jo["x"]?.Value<float>() ?? 0f,
            jo["y"]?.Value<float>() ?? 0f,
            jo["z"]?.Value<float>() ?? 0f,
            jo["w"]?.Value<float>() ?? 0f
        );
    }
}



public class Vector3Converter : JsonConverter<Vector3>
{
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.X);
        writer.WritePropertyName("y");
        writer.WriteValue(value.Y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.Z);
        writer.WriteEndObject();
    }

    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Vector3(
            jo["x"]?.Value<float>() ?? 0f,
            jo["y"]?.Value<float>() ?? 0f,
            jo["z"]?.Value<float>() ?? 0f
        );
    }
}




public class Vector2Converter : JsonConverter<Vector2>
{
    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.X);
        writer.WritePropertyName("y");
        writer.WriteValue(value.Y);
        writer.WriteEndObject();
    }

    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Vector2(
            jo["x"]?.Value<float>() ?? 0f,
            jo["y"]?.Value<float>() ?? 0f
        );
    }
}