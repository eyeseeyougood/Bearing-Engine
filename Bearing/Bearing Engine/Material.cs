using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Bearing;

public class Material
{
    [JsonConverter(typeof(ParameterConverter))]
    public Dictionary<string, object> parameters { get; set; }
    public Dictionary<string, int> attribs { get; set; }

    private int attribAllocCache;
    public Shader shader { get; set; }

    public Material() { }

    public void SetShaderParameter<T>(string name, T parameter)
    {
        // this can be done in a much smaller way (oneliner) by using reflection:
        // shader.GetType().GetMethod("Set"+kvp.Value.GetType().Name).Invoke(shader, new object[] { kvp.Key, kvp.Value });
        // but i prefer this as it is much easier to understand
        switch (parameter.GetType().Name)
        {
            case "Single":
                shader.SetFloat(name, Convert.ToSingle(parameter));
                break;
            case "Int32":
                shader.SetInt(name, Convert.ToInt32(parameter));
                break;
            case "Vector2":
                shader.SetVector2(name, (Vector2)Convert.ChangeType(parameter, typeof(Vector2)));
                break;
            case "Vector3":
                shader.SetVector3(name, (Vector3)Convert.ChangeType(parameter, typeof(Vector3)));
                break;
            case "Vector4":
                shader.SetVector4(name, (Vector4)Convert.ChangeType(parameter, typeof(Vector4)));
                break;
            case "Matrix4":
                shader.SetMatrix4(name, (Matrix4)Convert.ChangeType(parameter, typeof(Matrix4)));
                break;
        }
    }

    public void Use()
    {
        shader.Use();
    }

    public void LoadParameters()
    {
        parameters ??= new Dictionary<string, object>();

        foreach (var kvp in parameters)
        {
            SetShaderParameter(kvp.Key, kvp.Value);
        }
    }

    public void LoadAttribs()
    {
        attribAllocCache = 0;

        attribs ??= new Dictionary<string, int>();

        foreach (var kvp in attribs)
        {
            AllocAttribPointer(kvp.Key, kvp.Value);
        }
    }

    private void AllocAttribPointer(string name, int numFloats, bool normalised = false)
    {
        int texLoc = GL.GetAttribLocation(shader.GetHandle(), name);
        GL.VertexAttribPointer(texLoc, numFloats, VertexAttribPointerType.Float, normalised, MeshVertex3D.sizeInBytes, attribAllocCache * sizeof(float));
        GL.EnableVertexAttribArray(texLoc);
        attribAllocCache += numFloats;
    }
}