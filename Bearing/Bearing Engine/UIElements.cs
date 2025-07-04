using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class UIElement : MeshRenderer
{
    public UIElement(string mesh) : base(mesh, true) { }
    
    public int renderLayer { get; set; }

    protected UIElement _parent;

    public int parent { // id
        get
        {
            return _parent.rid;
        }
        set
        {
            _parent = UIManager.FindFromID(value);
            UpdatePosition();
        }
    }

    public Vector2 _anchor { get; set; } = new Vector2(0,0);
    public Vector2 anchor
    {
        get
        {
            return _anchor;
        }
        set
        {
            _anchor = value;
        }
    }

    public UDim2 _position { get; set; } = new UDim2(0.0f, 0.0f, 0, 0);
    public UDim2 position { 
        get {
            return _position;
        }
        set {
            _position = value;
            UpdatePosition();
        }
    }
    public UDim2 _size { get; set; } = new UDim2(0.0f,0.0f,200,200);
    public UDim2 size
    {
        get
        {
            return _size;
        }
        set
        {
            _size = value;
            UpdateSize();
        }

    }

    public Vector4i GetScreenBoundingBox()
    {
        Vector2 sizing = size.scale * Game.instance.ClientSize + size.offset;
        Vector2d positioning = position.scale * Game.instance.ClientSize + position.offset + sizing / 2 - anchor * sizing;

        Vector2d p1 = new Vector2d(-0.5f, -0.5f) * sizing + positioning;
        Vector2d p2 = new Vector2d(0.5f, 0.5f) * sizing + positioning;

        return new Vector4i((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y);
    }

    private void UpdatePosition()
    {
        if (_parent == null) return;

        Vector2 parentNormalisedScale = _parent.size.scale + (_parent.size.offset / Game.instance.ClientSize);

        Vector2 scale = _position.scale
                      * parentNormalisedScale
                      + _parent.position.scale
                      - _parent.anchor
                      * parentNormalisedScale;

        _position = new UDim2(scale, _position.offset);
    }

    private void UpdateSize()
    {
        if (_parent == null) return;

        _size = new UDim2(_size.scale * (_parent.size.scale + (_parent.size.offset / Game.instance.ClientSize)), _size.offset);
    }

    public override void OnLoad()
    {
        base.OnLoad();
        Game.instance.RemoveOpaqueRenderable(this); // ui should not be handled like all other renderables XDD
        UIManager.AddUI(this);
    }

    public override void OnTick(float dt)
    {
        UpdatePosition();
        UpdateSize();

        float screenW = Game.instance.Size.X;
        float screenH = Game.instance.Size.Y;

        material.SetShaderParameter(new ShaderParam("screenSize", Game.instance.ClientSize));
        material.SetShaderParameter(new ShaderParam("anchor", anchor));
        material.SetShaderParameter(new ShaderParam("posOffset", position.offset));
        material.SetShaderParameter(new ShaderParam("posScale", position.scale));
        material.SetShaderParameter(new ShaderParam("sizeOffset", size.offset));
        material.SetShaderParameter(new ShaderParam("sizeScale", size.scale));
    }
}

public class UIPanel : UIElement
{
    public UIPanel() : base("Quad.obj")
    {
        material = Material.uiFallback;
        setup3DMatrices = false;
        SetMesh(new Mesh2D("Quad.obj", true));
    }
}

public class UILabel : UIElement
{
    public UITheme theme = UIManager.currentTheme;

    private string _text = "Label";

    public string text
    {
        get
        {
            return _text;
        }
        set
        {
            _text = value;
            TextChanged(value);
        }
    }

    public bool fitHeightToWidth { get; set; } = true;

    private Texture texture;

    public event EventHandler<string> onTextChanged = (i,j) => { };

    public UILabel() : base("Quad.obj") { }

    public override void OnLoad()
    {
        material = new Material()
        {
            shader = new Shader("defaultUI.vert", "textureUI.frag"),
            attribs = new List<ShaderAttrib>()
            {
                new ShaderAttrib() { name = "aPosition", size = 2 },
                new ShaderAttrib() { name = "aTexCoord", size = 2 },
            },
            is3D = false
        };

        setup3DMatrices = false;
        SetMesh(new Mesh2D("Quad.obj", true));

        ResetTexture();

        base.OnLoad();
    }

    private void ResetTexture()
    {
        if (texture != null)
        {
            texture.Dispose();
        }

        texture = UIManager.UITextHelper.RenderTextToBmp(text);
    }

    protected virtual void TextChanged(string val)
    {
        onTextChanged.Invoke(this, val);

        ResetTexture();
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter(new ShaderParam("mainColour", theme.labelText.zeroToOne));
        material.SetShaderParameter(new ShaderParam("texSize", new Vector2(texture._width, texture._height)));
    }

    protected override void BeforeRender()
    {
        texture.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
    }
}

public class UIButton : UIElement
{
    private UITheme theme = UIManager.currentTheme;

    public event EventHandler buttonPressed = (i, j) => { };
    public event EventHandler buttonHold = (i, j) => { };
    public event EventHandler buttonReleased = (i, j) => { };

    public UIButton() : base("Quad.obj")
    {
        material = new Material()
        {
            shader = new Shader("defaultUI.vert", "defaultUI.frag"),
            attribs = new List<ShaderAttrib>()
            {
                new ShaderAttrib() { name = "aPosition", size = 2 },
                new ShaderAttrib() { name = "aTexCoord", size = 2 },
            },
            is3D = false
        };

        setup3DMatrices = false;
        SetMesh(new Mesh2D("Quad.obj", true));
    }

    private bool prevPressed = false;
    private bool pressed = false;

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        BearingColour bg = theme.buttonUpBackground;

        pressed = false;
        if (Extensions.PointInQuad(Game.instance.MousePosition, GetScreenBoundingBox()))
        {
            bg = theme.buttonHoverBackground;
            if (Input.GetMouseButton(0))
            {
                bg = theme.buttonDownBackground;
                pressed = true;
            }
        }

        if (pressed && !prevPressed)
        {
            // pressed this frame
            buttonPressed.Invoke(this, new EventArgs());
        }

        if (pressed)
        {
            // call hold
            buttonHold.Invoke(this, new EventArgs());
        }

        if (!pressed && prevPressed)
        {
            // released this frame
            buttonReleased.Invoke(this, new EventArgs());
        }

        prevPressed = pressed;

        // handle colour
        material.SetShaderParameter(new ShaderParam("mainColour", bg.zeroToOne));
    }
}

public class UIVerticalScrollView : UIElement
{
    private UITheme theme = UIManager.currentTheme;

    public List<int> contents { get; set; } = new List<int>();

    private float scroll;

    public UIVerticalScrollView() : base("Quad.obj")
    {
        material = new Material()
        {
            shader = new Shader("defaultUI.vert", "defaultUI.frag"),
            attribs = new List<ShaderAttrib>()
            {
                new ShaderAttrib() { name = "aPosition", size = 2 },
                new ShaderAttrib() { name = "aTexCoord", size = 2 },
            },
            is3D = false
        };

        setup3DMatrices = false;
        SetMesh(new Mesh2D("Quad.obj", true));
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        foreach (int item in contents)
        {
            UIElement? element = UIManager.FindFromID(item);

            if (element == null) { continue; }

            element.position = new UDim2(position.scale, Vector2.Zero);
            element.size = new UDim2(new Vector2(size.scale.X, element.size.scale.Y), element.size.offset);
        }

        position = new UDim2(MathF.Sin(Time.now), position.scale.Y);

        material.SetShaderParameter(new ShaderParam("mainColour", theme.verticalScrollBG.zeroToOne));
    }
}