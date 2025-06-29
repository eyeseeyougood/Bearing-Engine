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
    public Vector2 vector2 { get { return vV2; } set { vV2 = value; use = 2; } }

    private Vector3 vV3;
    public Vector3 vector3 { get { return vV3; } set { vV3 = value; use = 3; } }

    private Vector4 vV4;
    public Vector4 vector4 { get { return vV4; } set { vV4 = value; use = 4; } }

    private Matrix4 vM4;
    public Matrix4 matrix4{ get { return vM4; } set { vM4 = value; use = 5; } }

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
                return vector2;
            case 3:
                return vector3;
            case 4:
                return vector4;
            case 5:
                return matrix4;
        }

        return null;
    }

    public ShaderParam() { }

    public ShaderParam(string name, float @float)
    {
        this.name = name;
        this.@float = @float;
    }
    public ShaderParam(string name, int @int)
    {
        this.name = name;
        this.@int = @int;
    }
    public ShaderParam(string name, Vector2 vector2)
    {
        this.name = name;
        this.vector2 = vector2;
    }
    public ShaderParam(string name, Vector3 vector3)
    {
        this.name = name;
        this.vector3 = vector3;
    }
    public ShaderParam(string name, Vector4 vector4)
    {
        this.name = name;
        this.vector4 = vector4;
    }
    public ShaderParam(string name, Matrix4 matrix4)
    {
        this.name = name;
        this.matrix4 = matrix4;
    }
}

public class ShaderAttrib
{
    public string name { get; set; }
    public int size { get; set; }
}

public class Material
{
    public static Material fallback = new Material()
    {
        shader = new Shader("default.vert", "default.frag"),
        attribs = new List<ShaderAttrib>()
        {
            new ShaderAttrib() { name = "aPosition", size = 3 },
            new ShaderAttrib() { name = "aTexCoord", size = 2 },
            new ShaderAttrib() { name = "aNormal", size = 3 },
        },
        parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", vector4 = new Vector4(0.9f, 0.9f, 0.9f, 1.0f) },
        }
    };

    public static Material uiFallback = new Material()
    {
        shader = new Shader("defaultUI.vert", "defaultUI.frag"),
        attribs = new List<ShaderAttrib>()
        {
            new ShaderAttrib() { name = "aPosition", size = 2 },
            new ShaderAttrib() { name = "aTexCoord", size = 2 },
        },
        parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", vector4 = new Vector4(0.9f, 0.9f, 0.9f, 1.0f) },
        },
        is3D = false
    };

    public List<ShaderParam> parameters { get; set; }
    public List<ShaderAttrib> attribs { get; set; }
    public Shader shader { get; set; }
    public bool is3D { get; set; } = true;

    private int attribAllocCache;

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

    /// <summary>
    /// this is like the function you call to your friend asking to borrow their tooth brush but it actually turns out that they are busy rn so u hef to go through their window to pick up their toothbroosh but it turns out that all of the bristles from their toothbrush are missing and this is actually because they store their bristles seperately in their sefety vault just in case someone like u wanted to steel (Steel Engine the Engine Everybody Loves) their tooths from them while they were busy and so you have to go back to ur house and come up with a plan on how to get ur hands on their toothbrush, turns out that actually tmr they will be at a concert so u decide that is when u will strike, and so u go to their hus tmr and u find that in their haus they keep a slip of all of their passwords and stuffs and so u check and sure enough the slip has the code to their sefety vault and so u try it and it works and the latch clicks open and when u take a peek inside it turns out that they knew you would do this since you gave the phone a call and so they decided to take more measures against you, this is just more proof that ur firned dosdnt trust you with their tooths and brushes and so u need to get them back and so when they call about ur tooths and the brushes u simply tell them sure come by and pick it up since u busy, and actually it turns out that when they do come to pick up the toothbuoosrsh they fall through ur floor into another dimention because u just **had** to get them back and so they get hurt pretty bad but they say it is ok because you two are even steven and you move on.
    /// </summary>
    /// <param name="name">what dus ur sheder say abut the 'in vecc...'?</param>
    /// <param name="numFloats">how many floats for this pointer</param>
    /// <param name="normalised">do u want me to normalise ur data for you pall ???                                     XDD</param>
    private void AllocAttribPointer(string name, int numFloats, bool normalised = false)
    {
        int texLoc = GL.GetAttribLocation(shader.GetHandle(), name);
        GL.VertexAttribPointer(texLoc, numFloats, VertexAttribPointerType.Float, normalised, is3D ? MeshVertex3D.sizeInBytes : MeshVertex2D.sizeInBytes, attribAllocCache * sizeof(float));
        GL.EnableVertexAttribArray(texLoc);
        attribAllocCache += numFloats;
    }
}