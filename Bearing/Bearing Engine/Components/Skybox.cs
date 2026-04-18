using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing;

public class Skybox : MeshRenderer
{
	public Texture? cubeMap;

	public Skybox(Resource cubeMap) : base("Cube.obj", true)
	{
		this.cubeMap = Texture.LoadFromFile(cubeMap.fullpath);
	}

    public override void Cleanup()
    {
        base.OnLoad();
    }

    public override void OnLoad()
    {
		if (cubeMap is null)
		{
			Logger.LogError("Component 'Bearing.Skybox' cannot have empty 'cubeMap' field!");
			return;
		}

    	texture0 = cubeMap;

    	Material skyboxMaterial = new Material()
        {
            shader = new Shader("skybox.vert", "skybox.frag"),
            attribs = new List<ShaderAttrib>()
            {
                new ShaderAttrib() { name = "aPosition", size = 3 },
                new ShaderAttrib() { name = "aTexCoord", size = 2 },
                new ShaderAttrib() { name = "aNormal", size = 3 },
            },
            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", vector4 = new Vector4(0.9f, 0.9f, 0.9f, 1.0f) },
            },
            is3D = true,
        };

    	material = skyboxMaterial;
        renderPass = 1;
        setup3DMatrices = false;

        base.OnLoad();

        Logger.Log("initialised skybox!");
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter(new ShaderParam("view", Game.instance.camera.GetViewMatrix()));
        material.SetShaderParameter(new ShaderParam("projection", Game.instance.camera.GetProjectionMatrix()));
    }

    protected override void BeforeRender()
    {
        GL GL = GLContext.gl;
        GL.Disable(EnableCap.CullFace);

        if (cubeMap != null)
            texture0 = cubeMap;
    }

    protected override void AfterRender()
    {
        GL GL = GLContext.gl;
        GL.Enable(EnableCap.CullFace);
    }
}