using Assimp;
using Newtonsoft.Json;
using System.Drawing;
using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing;

public class ShaderParam // json bullshit
{
    public string name { get; set; }

    private List<object> v;
    public List<object> value { get { return v; } set { v = value; } }

    public object GetData()
    {
        float f(object x){if (x is double) {return Convert.ToSingle((double)x);} else {return (float)x;} }

        switch (value.Count)
        {
            case 1:
                return value[0];
            case 2:
                return new Vector2(f(value[0]), f(value[1]));
            case 3:
                return new Vector3(f(value[0]), f(value[1]), f(value[2]));
            case 4:
                return new Vector4(f(value[0]), f(value[1]), f(value[2]), f(value[3]));
            case 16:
                return new Matrix4(
                    f(value[0]), f(value[1]), f(value[2]), f(value[3]),
                    f(value[4]), f(value[5]), f(value[6]), f(value[7]),
                    f(value[8]), f(value[9]), f(value[10]), f(value[11]),
                    f(value[12]), f(value[13]), f(value[14]), f(value[15])
                    );
        }

        return value;
    }
}

public class ShaderAttrib
{
    public string name { get; set; }
    public int size { get; set; }
}

[InspectorShow]
public class Material
{
    public static Material fallback = new Material()
    {
        shader = new Shader("eng/default.vert", "eng/default.frag"),

        parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", value = new List<object> {0.9f, 0.9f, 0.9f, 1.0f} },
        }
    };

    public static Material uiFallback = new Material()
    {
        shader = new Shader("eng/defaultUI.vert", "eng/defaultUI.frag"),

        parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", value = new List<object> {0.9f, 0.9f, 0.9f, 1.0f} },
        },
    };

    public List<ShaderParam> parameters { get; set; } = new List<ShaderParam>();

    [HideFromInspector]
    public Shader shader { get; set; }

    public Material() { }

    public Material Clone()
    {
        Material clone = new Material();
        clone.shader = new Shader(shader.vert, shader.frag);
        clone.parameters = parameters.ToList();
        return clone;
    }

    public void SetShaderParameter(ShaderParam param)
    {
        SetShaderParameter(param.name, param.GetData());
    }

    public void SetShaderParameter(string name, object value)
    {
        // this can be done in a much smaller way (oneliner) by using reflection:
        // shader.GetType().GetMethod("Set"+kvp.Value.GetType().Name).Invoke(shader, new object[] { kvp.Key, kvp.Value });
        // but i prefer this as it is much easier to understand
        object data = value;
        switch (data.GetType().Name)
        {
            case "Single":
                shader.SetFloat(name, Convert.ToSingle(data));
                break;
            case "Int32":
                shader.SetInt(name, Convert.ToInt32(data));
                break;
            case "Vector2":
                shader.SetVector2(name, (Vector2)Convert.ChangeType(data, typeof(Vector2)));
                break;
            case "Vector3":
                shader.SetVector3(name, (Vector3)Convert.ChangeType(data, typeof(Vector3)));
                break;
            case "Vector4":
                shader.SetVector4(name, (Vector4)Convert.ChangeType(data, typeof(Vector4)));
                break;
            case "Matrix4":
                shader.SetMatrix4(name, (Matrix4)Convert.ChangeType(data, typeof(Matrix4)));
                break;
        }
    }

    public void Use()
    {
        shader.Use();
    }

    public void LoadParameters()
    {
        parameters ??= new List<ShaderParam>();

        foreach (var param in parameters)
        {
            string v = "";
            param.value.ToList().ForEach((i)=>{v+=i+" ";});
            SetShaderParameter(param);
        }
    }

    public void Cleanup()
    {
        shader.Cleanup();
        parameters.Clear();
    }
}