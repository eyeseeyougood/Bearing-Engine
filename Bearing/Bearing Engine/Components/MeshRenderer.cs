using Silk.NET.OpenGL;

namespace Bearing;

public class MeshRenderer : Renderable
{
    protected bool setup3DMatrices = true;

    public Texture texture0;
    public Texture texture1;
    public Texture texture2;

    protected uint ebo;
    protected uint vao;
    protected uint vbo;

    public MeshRenderer(string mesh)
    {
        this.mesh = new Mesh3D(Resource.GetModel(mesh));
    }

    private MeshRenderer() {}

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

        material.Use();

        base.OnLoad();

        Logger.Log("Loaded mesh renderer!");
    }

    public override void OnTick(float dt)
    {
    }

    public override unsafe void Render()
    {
        GL GL = GLContext.gl;

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        if (setup3DMatrices)
        {
            material.SetShaderParameter("model", ((Transform3D)gameObject.transform).GetModelMatrix());
            material.SetShaderParameter("view", Game.instance.camera.GetViewMatrix());
            material.SetShaderParameter("projection", Game.instance.camera.GetProjectionMatrix());
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

        AfterRender();
    }

    protected virtual void BeforeRender() { }

    protected virtual void AfterRender() { }

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

        base.Cleanup();
    }
}