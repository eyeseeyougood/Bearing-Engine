using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Bearing;

public class MeshRenderer : Component, IRenderable
{
    public Mesh mesh { get; private set; }
    public Material material { get; set; }
    public int id { get; set; }

    protected bool setup3DMatrices = true;

    private int ebo;
    private int vao;
    private int vbo;

    public MeshRenderer(string mesh, bool meshIsEngineResource = false)
    {
        this.mesh = new Mesh3D(mesh, meshIsEngineResource);
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

    public void Render()
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

        GL.DrawElements(PrimitiveType.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    protected void SetMesh(Mesh nMesh)
    {
        mesh = nMesh;
    }

    public override void Cleanup()
    {
    }
}