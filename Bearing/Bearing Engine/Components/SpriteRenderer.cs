using Silk.NET.OpenGL;

namespace Bearing;

public class SpriteRenderer : Component, IRenderable
{
    [HideFromInspector] public Mesh2D mesh { get; private set; }
    public Material material { get; set; } = Material.fallback;
    [HideFromInspector] public int rid { get; set; } = -1;

    public Sprite sprite;
    public bool renderBackface = false;

    private uint ebo;
    private uint vao;
    private uint vbo;

    public SpriteRenderer()
    {
        mesh = new Mesh2D("Quad.obj", true);
    }

    public override void OnLoad()
    {
        GL GL = GLContext.gl;

        float[] vertexData = mesh.GetVertexData();

        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexData.Length * sizeof(float)), new ReadOnlySpan<float>(vertexData), BufferUsageARB.StaticDraw);

        ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(mesh.indices.Length * sizeof(uint)), new ReadOnlySpan<uint>(mesh.indices), BufferUsageARB.StaticDraw);

        material.LoadAttribs();

        Game.instance.AddOpaqueRenderable(this); // make this recieve the render call
    }

    public override void OnTick(float dt) {}
 
    public virtual unsafe void Render()
    {
        GL GL = GLContext.gl;

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        Transform2D transform = ((Transform2D)gameObject.transform);
        material.SetShaderParameter(new ShaderParam("position", transform.position));
        material.SetShaderParameter(new ShaderParam("rot", transform.rotation));
        material.SetShaderParameter(new ShaderParam("scale", transform.scale));
        material.SetShaderParameter(new ShaderParam("view", Game.instance.camera.GetViewMatrix()));
        material.SetShaderParameter(new ShaderParam("projection", Game.instance.camera.GetProjectionMatrix()));

        material.LoadParameters();

        LightManager.AddLightingInfo(material);

        Texture t = sprite.Peak();

        if (t != null)
            t.Use(TextureUnit.Texture0);

        if (renderBackface)
        {
            GL.Disable(GLEnum.CullFace);
        }

        BeforeRender();

        material.Use();

        GL.DrawElements(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    protected virtual void BeforeRender() { }

    public override void Cleanup()
    {
        GL GL = GLContext.gl;
        
        GL.DeleteBuffer(ebo);
        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);

        if (sprite != null)
            sprite.Cleanup();

        material.Cleanup();

        Game.instance.RemoveOpaqueRenderable(this);
    }
}