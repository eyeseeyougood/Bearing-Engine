using Silk.NET.OpenGL;

namespace Bearing;

public class MeshRenderer : Component, IRenderable
{
    [HideFromInspector] public Mesh mesh { get; private set; }
    public Material material { get; set; } = Material.fallback;
    [HideFromInspector] public int rid { get; set; } = -1;

    protected bool setup3DMatrices = true;

    public Texture texture0;
    public Texture texture1;
    public Texture texture2;

    private uint ebo;
    private uint vao;
    private uint vbo;

    public MeshRenderer(string mesh, bool meshIsEngineResource = false, bool skipMesh = false)
    {
        if (!skipMesh)
            this.mesh = new Mesh3D(mesh, meshIsEngineResource);
    }

    private MeshRenderer() {}

    public static MeshRenderer FromMesh(Mesh _mesh) { return new MeshRenderer() { mesh = _mesh }; }

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

    public override void OnTick(float dt)
    {
    }

    public virtual unsafe void Render()
    {
        GL GL = GLContext.gl;

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        if (setup3DMatrices)
        {
            material.SetShaderParameter(new ShaderParam("model", gameObject.transform.GetModelMatrix()));
            material.SetShaderParameter(new ShaderParam("view", Game.instance.camera.GetViewMatrix()));
            material.SetShaderParameter(new ShaderParam("projection", Game.instance.camera.GetProjectionMatrix()));
        }

        material.LoadParameters();

        LightManager.AddLightingInfo(material);

        if (texture0 != null)
            texture0.Use(TextureUnit.Texture0);

        if (texture1 != null)
            texture1.Use(TextureUnit.Texture1);

        if (texture2 != null)
            texture2.Use(TextureUnit.Texture2);

        BeforeRender();

        material.Use();

        GL.DrawElements(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    protected virtual void BeforeRender() { }

    protected void SetMesh(Mesh nMesh)
    {
        mesh = nMesh;
    }

    public override void Cleanup()
    {
        GL GL = GLContext.gl;

        GL.DeleteBuffer(ebo);
        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);

        if (texture0 != null)
            texture0.Dispose();
        
        if (texture1 != null)
            texture1.Dispose();

        if (texture2 != null)
            texture2.Dispose();

        material.Cleanup();

        Game.instance.RemoveOpaqueRenderable(this);
    }
}