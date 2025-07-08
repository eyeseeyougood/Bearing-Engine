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
    public UIElement(string mesh) : base(mesh, true) { UIManager.AddUI(this); }

    public int renderLayer { get; set; }

    public Action positionChanged = () => { };
    public Action sizeChanged = () => { };

    protected UIElement _parent;

    public int parent { // id
        get
        {
            if (_parent != null)
                return _parent.rid;

            return -1;
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

    public UDim2 _setPos{ get; set; } = new UDim2(0.0f, 0.0f, 0, 0);
    public UDim2 _position { get; set; } = new UDim2(0.0f, 0.0f, 0, 0);
    public UDim2 position { 
        get {
            return _position;
        }
        set {
            _setPos = value;
            UpdatePosition();
            positionChanged.Invoke();
        }
    }
    public UDim2 _setSize { get; set; } = new UDim2(0.0f,0.0f,200,200);
    public UDim2 _size { get; set; } = new UDim2(0.0f,0.0f,200,200);
    public UDim2 size
    {
        get
        {
            return _size;
        }
        set
        {
            _setSize = value;
            UpdateSize();
            sizeChanged.Invoke();
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

    /// <summary>
    /// Force a position recalculation
    /// </summary>
    public void UpdatePosition()
    {
        if (_parent == null) { _position = _setPos; return; }

        Vector2 parentNormalisedScale = _parent.size.scale + (_parent.size.offset / Game.instance.ClientSize);

        Vector2 scale = _setPos.scale
                      * parentNormalisedScale
                      + _parent.position.scale
                      - _parent.anchor
                      * parentNormalisedScale;

        _position = new UDim2(scale, _setPos.offset);
    }

    /// <summary>
    /// Force a size recalculation
    /// </summary>
    public void UpdateSize()
    {
        if (_parent == null) { _size = _setSize; return; }

        _size = new UDim2(_setSize.scale * (_parent.size.scale + (_parent.size.offset / Game.instance.ClientSize)), _setSize.offset);
    }

    public override void OnLoad()
    {
        base.OnLoad();

        if (_parent != null)
        {
            _parent.positionChanged += UpdatePosition;
            _parent.sizeChanged += UpdateSize;
        }

        Game.instance.RemoveOpaqueRenderable(this); // ui should not be handled like all other renderables XDD
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
        if (texture0 != null)
        {
            texture0.Dispose();
        }

        texture0 = UIManager.UITextHelper.RenderTextToBmp(text);
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
        material.SetShaderParameter(new ShaderParam("texSize", new Vector2(texture0._width, texture0._height)));
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

        material.SetShaderParameter(new ShaderParam("mainColour", theme.verticalScrollBG.zeroToOne));
    }

    protected override void BeforeRender()
    {
        base.BeforeRender();

        foreach (int item in contents)
        {
            UIElement? element = UIManager.FindFromID(item);

            if (element == null) { continue; }

            element.position = new UDim2(position.scale, Vector2.Zero);
            element.size = new UDim2(new Vector2(size.scale.X, element.size.scale.Y), element.size.offset);
        }
    }
}