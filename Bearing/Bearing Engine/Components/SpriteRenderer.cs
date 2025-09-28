using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Bearing;

public class SpriteRenderer : Component, IRenderable
{
    [HideFromInspector] public Mesh2D mesh { get; private set; }
    public Material material { get; set; } = Material.fallback;
    [HideFromInspector] public int rid { get; set; } = -1;

    protected bool setup3DMatrices = true;

    public Sprite sprite;

    private int ebo;
    private int vao;
    private int vbo;

    public SpriteRenderer(string mesh, bool meshIsEngineResource = false, bool skipMesh = false)
    {
        if (!skipMesh)
            this.mesh = new Mesh2D(mesh, meshIsEngineResource);
    }

    public override void OnLoad()
    {
        float[] vertexData = mesh.GetVertexData();

        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

        ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.indices.Length * sizeof(uint), mesh.indices, BufferUsageHint.StaticDraw);

        material.LoadAttribs();

        Game.instance.AddOpaqueRenderable(this); // make this recieve the render call
    }

    public override void OnTick(float dt)
    {
    }

    public virtual void Render()
    {
        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

        if (setup3DMatrices)
        {
            material.SetShaderParameter(new ShaderParam("model", gameObject.transform.GetModelMatrix()));
            material.SetShaderParameter(new ShaderParam("view", Game.instance.camera.GetViewMatrix()));
            material.SetShaderParameter(new ShaderParam("projection", Game.instance.camera.GetProjectionMatrix()));
        }

        material.LoadParameters();

        LightManager.AddLightingInfo(material);

        Texture t = sprite.Peak();

        if (t != null)
            t.Use(TextureUnit.Texture0);

        BeforeRender();

        material.Use();

        GL.DrawElements(PrimitiveType.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    protected virtual void BeforeRender() { }

    protected void SetMesh(Mesh2D nMesh)
    {
        mesh = nMesh;
    }

    public override void Cleanup()
    {
        GL.DeleteBuffer(ebo);
        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);

        if (sprite != null)
            sprite.Cleanup();

        material.Cleanup();

        Game.instance.RemoveOpaqueRenderable(this);
    }
}