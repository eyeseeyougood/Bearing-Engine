using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Bearing;

public class MeshRenderer : Component, IRenderable
{
    public Mesh mesh { get; }
    public Material material { get; set; }
    public int id { get; set; }

    private int ebo;
    private int vao;
    private int vbo;

    public MeshRenderer(string mesh)
    {
        this.mesh = new Mesh(mesh);
    }

    public override void OnLoad()
    {
        float[] vertexData = mesh.GetVertexData();

        gameObject.transform.GetModelMatrix();

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

    public override void OnTick()
    {
    }

    public void Render()
    {
        GL.Enable(EnableCap.DepthTest);

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

        material.SetShaderParameter(new ShaderParam("model", gameObject.transform.GetModelMatrix()));
        material.SetShaderParameter(new ShaderParam("view", Game.instance.camera.GetViewMatrix()));
        material.SetShaderParameter(new ShaderParam("projection", Game.instance.camera.GetProjectionMatrix()));

        material.LoadParameters();

        GL.DrawElements(PrimitiveType.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public override void Cleanup()
    {
    }
}