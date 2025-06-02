using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;

namespace Bearing;

public class ShaderParam // json bullshit
{
    public string name { get; set; }

    private float vF;
    public float @float { get { return vF; } set { vF = value; use = 0; } }

    private int vI;
    public int @int { get { return vI; } set { vI = value; use = 1; } }

    private Vector2 vV2;
    public Vector2 @Vector2 { get { return vV2; } set { vV2 = value; use = 2; } }

    private Vector3 vV3;
    public Vector3 @Vector3 { get { return vV3; } set { vV3 = value; use = 3; } }

    private Vector4 vV4;
    public Vector4 @Vector4 { get { return vV4; } set { vV4 = value; use = 4; } }

    private Matrix4 vM4;
    public Matrix4 @Matrix4{ get { return vM4; } set { vM4 = value; use = 5; } }

    private int use;

    public object GetData()
    {
        switch (use)
        {
            case 0:
                return @float;
            case 1:
                return @int;
            case 2:
                return @Vector2;
            case 3:
                return @Vector3;
            case 4:
                return @Vector4;
            case 5:
                return @Matrix4;
        }

        return null;
    }

    public ShaderParam()
    {
        name = "";
    }
}

public class ShaderAttrib
{
    public string name { get; set; }
    public int size { get; set; }

    public ShaderAttrib()
    {
        name = "";
        size = 0;
    }
}

public class Material
{
    public List<ShaderParam> parameters { get; set; }
    public List<ShaderAttrib> attribs { get; set; }

    private int attribAllocCache;
    public Shader shader { get; set; }

    public Material() { }

    public void SetShaderParameter(ShaderParam param)
    {
        // this can be done in a much smaller way (oneliner) by using reflection:
        // shader.GetType().GetMethod("Set"+kvp.Value.GetType().Name).Invoke(shader, new object[] { kvp.Key, kvp.Value });
        // but i prefer this as it is much easier to understand
        object data = param.GetData();
        switch (data.GetType().Name)
        {
            case "Single":
                shader.SetFloat(param.name, Convert.ToSingle(data));
                break;
            case "Int32":
                shader.SetInt(param.name, Convert.ToInt32(data));
                break;
            case "Vector2":
                shader.SetVector2(param.name, (Vector2)Convert.ChangeType(data, typeof(Vector2)));
                break;
            case "Vector3":
                shader.SetVector3(param.name, (Vector3)Convert.ChangeType(data, typeof(Vector3)));
                break;
            case "Vector4":
                shader.SetVector4(param.name, (Vector4)Convert.ChangeType(data, typeof(Vector4)));
                break;
            case "Matrix4":
                shader.SetMatrix4(param.name, (Matrix4)Convert.ChangeType(data, typeof(Matrix4)));
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
            SetShaderParameter(param);
        }
    }

    public void LoadAttribs()
    {
        attribAllocCache = 0;

        attribs ??= new List<ShaderAttrib>();

        foreach (var attrib in attribs)
        {
            AllocAttribPointer(attrib.name, attrib.size);
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