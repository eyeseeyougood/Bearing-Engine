using Silk.NET.OpenGL;
namespace Bearing;

public class Renderable : Component
{
    public Material material { get; set; } = Material.fallback;

    /// <summary>Call at end of override, ENSURE MATERIAL IS SET BEFORE CALLING</summary>
    public override void OnLoad(){ material.Use(); LoadAttribs(); Game.instance.AddRenderable(this); }
    public override void OnTick(float dt){}
    public override void Cleanup(){ Game.instance.RemoveRenderable(this); }

    public int rid { get; set; } = -1;
    public int renderPass { get; set; } = 0;
    public bool useTransparency { get; set; } = false;

    protected Mesh? mesh;

    protected int attribAllocCache;

    public Renderable() { rid = Game.instance.GetUniqueRenderableID(); }

    public Mesh? GetMesh()
    {
        return mesh;
    }

    /// <summary>
    /// this is like the function you call to your friend asking to borrow their tooth brush but it actually turns out that they are busy rn so u hef to go through their window to pick up their toothbroosh but it turns out that all of the bristles from their toothbrush are missing and this is actually because they store their bristles seperately in their sefety vault just in case someone like u wanted to steel (Steel Engine the Engine Everybody Loves) their tooths from them while they were busy and so you have to go back to ur house and come up with a plan on how to get ur hands on their toothbrush, turns out that actually tmr they will be at a concert so u decide that is when u will strike, and so u go to their hus tmr and u find that in their haus they keep a slip of all of their passwords and stuffs and so u check and sure enough the slip has the code to their sefety vault and so u try it and it works and the latch clicks open and when u take a peek inside it turns out that they knew you would do this since you gave the phone a call and so they decided to take more measures against you, this is just more proof that ur firned dosdnt trust you with their tooths and brushes and so u need to get them back and so when they call about ur tooths and the brushes u simply tell them sure come by and pick it up since u busy, and actually it turns out that when they do come to pick up the toothbuoosrsh they fall through ur floor into another dimention because u just **had** to get them back and so they get hurt pretty bad but they say it is ok because you two are even steven and you move on.
    /// </summary>
    /// <param name="name">what dus ur sheder say abut the 'in vecc...'?</param>
    /// <param name="numFloats">how many floats for this pointer</param>
    /// <param name="normalised">do u want me to normalise ur data for you pall ???                                     XDD</param>
    protected unsafe void AllocAttribPointer(string name, int numFloats, bool normalised = false)
    {
        GL GL = GLContext.gl;

        GL.GetInteger(GLEnum.CurrentProgram, out int shaderHandle);

        if (shaderHandle == 0)
        {
            Logger.LogError("Cannot allocate attribute pointer due to there not being a target shader bound!!!");
        }

        bool is3D = mesh is Mesh3D;

        int texLoc = GL.GetAttribLocation((uint)shaderHandle, name);
        GL.VertexAttribPointer((uint)texLoc, numFloats, VertexAttribPointerType.Float, normalised, (uint)(is3D ? MeshVertex3D.sizeInBytes : MeshVertex2D.sizeInBytes), (void*)(attribAllocCache * sizeof(float)));
        GL.EnableVertexAttribArray((uint)texLoc);

        attribAllocCache += numFloats;
    }

    protected virtual void LoadAttribs()
    {
        attribAllocCache = 0;

        if (mesh is null)
        {
            Logger.LogError("Attempt to load GL attributes before setting the mesh");
            return;
        }

        ShaderAttrib[] attribs = mesh.GetAttributes();

        foreach (var attrib in attribs)
        {
            AllocAttribPointer(attrib.name, attrib.size);
        }
    }

    public virtual unsafe void Render() { }
}