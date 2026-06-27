using Silk.NET.OpenGL;

namespace Bearing;

public class SpriteRenderer : Renderable
{
    protected static Mesh2D quad;

    public Sprite sprite = new Sprite();
    public bool renderBackface = false;

    protected uint ebo;
    protected uint vao;
    protected uint vbo;

    public SpriteRenderer() : base()
    {
        if (quad == null)
            quad = new Mesh2D(Resource.GetModel("eng/Quad.obj"));

        mesh = quad;
        material = new Material()
        {
            shader = new Shader("eng/default2D.vert", "eng/texture.frag"),

            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", value = new List<object> {0.9f, 0.9f, 0.9f, 1.0f} },
            },
        };
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

        base.OnLoad();
    }

    public override void OnTick(float dt) {}
 
    public override unsafe void Render()
    {
        GL GL = GLContext.gl;

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        Transform2D transform = ((Transform2D)gameObject.transform);
        material.SetShaderParameter("position", transform.position);
        material.SetShaderParameter("rot", transform.rotation);
        material.SetShaderParameter("scale", transform.scale);
        material.SetShaderParameter("view", Game.instance.camera.GetViewMatrix());
        material.SetShaderParameter("projection", Game.instance.camera.GetProjectionMatrix());

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

        base.Cleanup();
    }
}