using OpenTK.Mathematics;

namespace Bearing;

public class FancyUIButton : UIElement
{
	public UIImage image;
	public UILabel text;
	public UIButton button;

    private Material squircleMat;

    public FancyUIButton() : base()
    {
        material = new Material()
        {
            shader = new Shader("defaultUI.vert", "textureUI.frag"),
            attribs = new List<ShaderAttrib>()
            {
                new ShaderAttrib() { name = "aPosition", size = 2 },
                new ShaderAttrib() { name = "aTexCoord", size = 2 },
            },
            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", vector4 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) },
            },
            is3D = false
        };

        squircleMat = new Material()
        {
            shader = new Shader("defaultUI.vert", "squircle.frag"),
            attribs = new List<ShaderAttrib>()
            {
                new ShaderAttrib() { name = "aPosition", size = 2 },
                new ShaderAttrib() { name = "aTexCoord", size = 2 },
            },
            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", vector4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) },
            },
            is3D = false
        };
    }

    public override void OnLoad()
    {
        base.OnLoad();

        image = new UIImage();
        image.anchor = new Vector2(0.5f, 0.5f);
        image.position = new UDim2(-0.5f, 0.5f);
        image.size = new UDim2(1f, 1f);
        image.material = squircleMat.Clone();
        image.material.SetShaderParameter(new ShaderParam("radius", 0.5f));
        image.SetTexture(Texture.LoadFromFile("./EngineData/Textures/Blank.png"));
        gameObject.AddComponent(image);

        text = new UILabel();
        text.anchor = new Vector2(0.5f, 0.5f);
        text.position = new UDim2(0.5f, 0.5f);
        text.size = new UDim2(1f, 1f, -20, -20);
        text.text = "Fancy button!";
        gameObject.AddComponent(text);

        button = new UIButton();
        button.anchor = new Vector2(0.5f, 0.5f);
        button.position = new UDim2(0.5f, 0.5f);
        button.size = new UDim2(1f, 1f);
        gameObject.AddComponent(button);

        image.parent = rid;
        text.parent = rid;
        button.parent = rid;
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        image.renderLayer = renderLayer + 1;
    }
}