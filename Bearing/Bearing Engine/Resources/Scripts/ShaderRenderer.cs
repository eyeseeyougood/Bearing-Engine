using OpenTK.Mathematics;
using Silk.NET.OpenGL;
using Bearing;

public class ShaderImage : UIElement
{
    private int colTex = -1;

    public ShaderImage(params object[] meta) : base(meta)
    {
        material = new Material()
        {
            shader = new Bearing.Shader("eng/defaultUI.vert", "eng/textureUI.frag"),
            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", value = new List<object> {1.0f, 1.0f, 1.0f, 1.0f} },
            },
        };
    }

    public void SetTexture(int handle)
    {
        this.colTex = handle;
    }

    public override unsafe void Render()
    {
        UpdateVisibility();

        if (!visible)
            return;

        GL GL = GLContext.gl;

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        material.SetShaderParameter("screenSize", Game.instance.ClientSize);
        material.SetShaderParameter("anchor", anchor);
        material.SetShaderParameter("posOffset", worldPosition.offset);
        material.SetShaderParameter("posScale", worldPosition.scale);
        material.SetShaderParameter("sizeOffset", worldSize.offset);
        material.SetShaderParameter("sizeScale", worldSize.scale);

        material.LoadParameters();

        if (colTex != -1)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, (uint)colTex);
        }

        BeforeRender();

        material.Use();

        GL.DrawElements(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter("mainColour", Vector4.One);
        material.SetShaderParameter("fitToTexRatio", 0);

        Bearing.Texture tex = sprite.Peak();

        if (tex != null)
            material.SetShaderParameter("texSize", new Vector2(tex._width, tex._height));
    }
}

public class ShaderRenderer : MeshRenderer
{
	private ScreenTexture? screenTexture;

    private GameObject? renderTarget;
    private MeshRenderer? renderTargetMr;
    private ShaderImage image;

    public ShaderRenderer(ShaderImage image) : base("eng/Quad.obj") { this.image = image; }

    public override void OnLoad()
    {
    	base.OnLoad();

        screenTexture = ScreenTexture.CreateEmpty();

        Game.instance.beforeRender += BindDepthTexture;

        renderTarget = new GameObject();
        renderTarget.Transform3D().position = new Vector3(0f, 0f, -1.5f);
        renderTarget.Load();
        MeshRenderer mr = new MeshRenderer("eng/Suzanne.obj");
        mr.material = new Material()
        {
            shader = new Bearing.Shader("eng/default.vert", "eng/lighting.frag"),
            parameters = new List<ShaderParam>() { new ShaderParam() {name="mainColour",value=new List<object>() {1.0f,1.0f,1.0f,1.0f}} }
        };
        renderTarget.AddComponent(mr);
        Game.instance.RemoveRenderable(mr);

        renderTargetMr = mr;

        gameObject.Transform3D().scale = new Vector3(1.6f, 0.9f, 1f);
    }

    public void BindDepthTexture()
    {
        if (screenTexture is null)
            return;

        screenTexture.Bind();
        GLContext.gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        RenderTargetObject();
    }

    private void RenderTargetObject()
    {
        if (renderTargetMr is null)
            return;

        if (screenTexture is null)
            return;

        Vector3 initPosition = Game.instance.camera.Position;
        float initYaw = Game.instance.camera.Yaw;
        float initPitch = Game.instance.camera.Pitch;

        Game.instance.camera.Position = new Vector3(0,0,0);
        Game.instance.camera.Yaw = -90f;
        Game.instance.camera.Pitch = 0f;

        renderTargetMr.Render();

        Game.instance.camera.Position = initPosition;
        Game.instance.camera.Yaw = initYaw;
        Game.instance.camera.Pitch = initPitch;

        screenTexture.Unbind();
    }

    public override void Render()
    {
        if (screenTexture is null)
            return;

        if (image is null)
            return;

        image.SetTexture((int)screenTexture.GetColourHandle());
    }

    public override void Cleanup()
    {
        base.Cleanup();
        
        Game.instance.beforeRender -= BindDepthTexture;
        
        if (screenTexture is null)
            return;
        
        screenTexture.Dispose();
    }
}