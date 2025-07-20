using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearing;

public class UIElement : MeshRenderer
{
    public UIElement(string mesh) : base(mesh, true) { UIManager.AddUI(this); }

    public List<string> consumedInputs = new List<string>();

    public int renderLayer { get; set; }

    private bool _setVisible = true;
    private bool _visible = true;
    public bool visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _setVisible = value;
            UpdateVisibility();
        }
    }

    public Action positionChanged = () => { };
    public Action sizeChanged = () => { };
    public Action onCleanup = () => { };

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

    protected virtual void UpdateVisibility()
    {
        if (!_setVisible)
        {
            _visible = false;
            return;
        }

        if (_parent == null)
        {
            _visible = _setVisible;
            return;
        }

        _visible = _parent.visible;
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

        _position = new UDim2(scale, _setPos.offset + _parent.position.offset);
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
            _parent.onCleanup += ParentCleanedUp;
        }

        Game.instance.RemoveOpaqueRenderable(this); // ui should not be handled like all other renderables XDD
    }

    protected virtual void UpdateShaderParams()
    {
        material.SetShaderParameter(new ShaderParam("screenSize", Game.instance.ClientSize));
        material.SetShaderParameter(new ShaderParam("anchor", anchor));
        material.SetShaderParameter(new ShaderParam("posOffset", position.offset));
        material.SetShaderParameter(new ShaderParam("posScale", position.scale));
        material.SetShaderParameter(new ShaderParam("sizeOffset", size.offset));
        material.SetShaderParameter(new ShaderParam("sizeScale", size.scale));
    }

    public override void OnTick(float dt)
    {
        UpdatePosition();
        UpdateSize();

        float screenW = Game.instance.Size.X;
        float screenH = Game.instance.Size.Y;
    }

    public void ParentCleanedUp()
    {
        gameObject.RemoveComponent(this);
        Cleanup();
    }

    public override void Cleanup()
    {
        base.Cleanup();

        onCleanup.Invoke();
        UIManager.RemoveUI(this);
    }

    public override void Render()
    {
        UpdateVisibility();
        UpdateShaderParams();

        if (visible)
            base.Render();
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
    public string font { get; set; } = "Arial";

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

    protected void ResetTexture()
    {
        if (texture0 != null)
        {
            texture0.Dispose();
        }

        texture0 = UIManager.UITextHelper.RenderTextToBmp(text, font);
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

public class UITextBox : UILabel
{
    public UITheme theme = UIManager.currentTheme;

    private UITheme buttonTheme = new UITheme();

    private UIButton button;

    private bool selected = false;

    public UITextBox() : base() { }

    public override void OnLoad()
    {
        buttonTheme.buttonUpBackground = theme.selection;
        buttonTheme.buttonDownBackground = theme.selection;
        buttonTheme.buttonHoverBackground = theme.selection;

        button = new UIButton()
        {
            parent = rid,
            renderLayer = 0,

            theme = buttonTheme,

            anchor = new Vector2(0.5f, 0.5f),

            position = new UDim2(0.5f, 0.5f),

            size = new UDim2(1, 1),
        };
        gameObject.AddComponent(button);

        button.buttonPressed += Pressed;

        base.OnLoad();

        Input.onCharacterPressed += OnCharacterPressed;
    }

    private void Pressed(object? sender, EventArgs e)
    {
        selected = true;
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        button.theme = selected ? buttonTheme : theme;

        if (Input.GetKeyDown(Keys.Backspace))
        {
            text = string.Join("",text.SkipLast(1));
        }
        if (Input.GetKeyDown(Keys.Enter) && Input.GetKey(Keys.LeftShift))
        {
            text += "\n";
        }
        else if ((Input.GetKeyDown(Keys.Escape) || Input.GetKeyDown(Keys.Enter)) && selected)
        {
            selected = false;
        }
    }

    private void OnCharacterPressed(string character)
    {
        if (!selected)
            return;

        text += character[0];

        ResetTexture();
    }
}

public class UIButton : UIElement
{
    public UITheme theme = UIManager.currentTheme;

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
        if (Extensions.PointInQuad(Game.instance.MousePosition, GetScreenBoundingBox()) && visible)
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
    public UITheme theme = UIManager.currentTheme;

    public int scrollSensitivity { get; set; } = 1;
    public int spacing { get; set; } = 5;

    public List<int> contents { get; set; } = new List<int>();

    private int scroll;
    private float scrollOffset;

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

        consumedInputs.Add("Scroll");
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);
        
        if (!ChildAbsorbedScroll() && MathF.Abs(Input.GetMouseScrollDelta().Y) > 0)
            if (Extensions.PointInQuad(Game.instance.MousePosition, GetScreenBoundingBox()))
            {
                int delta = (int)Input.GetMouseScrollDelta().Y;

                int numInvis = GetInvisibleElements();

                scroll += delta * scrollSensitivity;

                if (-scroll >= contents.Count || -scroll < 0 || numInvis == 0)
                    scroll -= delta * scrollSensitivity;
            }

        material.SetShaderParameter(new ShaderParam("mainColour", theme.verticalScrollBG.zeroToOne));
    }

    private bool ChildAbsorbedScroll()
    {
        bool result = false;

        foreach (int el in contents)
        {
            UIElement elem = UIManager.FindFromID(el);

            if (!elem.consumedInputs.Contains("Scroll"))
                continue;

            if (Extensions.PointInQuad(Game.instance.MousePosition, elem.GetScreenBoundingBox()))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    public void ClearContents()
    {
        foreach (int elem in contents.ToList())
        {
            UIElement element = UIManager.FindFromID(elem);

            if (element != null)
                element.gameObject.RemoveComponent(element);
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

        ClearContents();
    }

    public void SetScrollAmount(int amount)
    {
        scroll = amount;
    }

    public int GetScrollAmount()
    {
        return scroll;
    }

    protected override void UpdateVisibility()
    {
        base.UpdateVisibility();

        foreach (int item in contents)
        {
            UIElement? element = UIManager.FindFromID(item);

            element.visible = false;
        }
    }

    // TODO: OPTIMISATION - can make this better by just keeping a cache variable that increments and decrements from the beforerender function
    private int GetInvisibleElements()
    {
        int result = 0;

        foreach (int item in contents)
        {
            UIElement? element = UIManager.FindFromID(item);

            if (!element.visible)
            {
                result++;
            }
        }

        return result;
    }

    private void UpdateScrollOffset()
    {
        scrollOffset = 0;
        for (int i = 0; i < -scroll; i++)
        {
            UIElement el = UIManager.FindFromID(contents[i]);
            Vector2 normalisedScale = el.size.scale + (el.size.offset / Game.instance.ClientSize);
            scrollOffset += normalisedScale.Y;
        }

        scrollOffset += spacing / (float)Game.instance.ClientSize.Y * -scroll;
    }

    protected override void BeforeRender()
    {
        base.BeforeRender();

        UpdateScrollOffset();

        int index = 0;
        UIElement prevElement = null;
        foreach (int item in contents)
        {
            UIElement? element = UIManager.FindFromID(item);

            if (element == null) { continue; }

            float newIndex = index + scroll;

            Vector2 normalisedScale = size.scale + (size.offset / Game.instance.ClientSize);
            Vector2 elementNormalisedScale = element.size.scale + (element.size.offset / Game.instance.ClientSize);
            Vector2 prevElementNormalisedScale = elementNormalisedScale;

            Vector2 prevElementNormalisedPos = new Vector2(0, -scrollOffset - elementNormalisedScale.Y) + (position.offset-Vector2.UnitY*spacing) / Game.instance.ClientSize;
            if (prevElement != null)
            {
                prevElementNormalisedScale = prevElement.size.scale + (prevElement.size.offset / Game.instance.ClientSize);
                prevElementNormalisedPos = prevElement.position.scale + (prevElement.position.offset / Game.instance.ClientSize);
            }

            float elementOffset = prevElementNormalisedPos.Y * Game.instance.ClientSize.Y + prevElementNormalisedScale.Y * Game.instance.ClientSize.Y;

            element.position = new UDim2(position.scale, new Vector2(0, spacing + elementOffset));
            element.size = new UDim2(new Vector2(size.scale.X, element.size.scale.Y), element.size.offset + new Vector2(size.offset.X, 0));

            element.anchor = anchor;

            // check if still in bounding box otherwise dont render

            Vector4 ebb = element.GetScreenBoundingBox();

            Vector4 obb = GetScreenBoundingBox();

            bool shouldRender = true;
            if (!Extensions.PointInQuad(ebb.Xy, obb))
                shouldRender = false;
            else if (!Extensions.PointInQuad(ebb.Zw, obb))
                shouldRender = false;

            element.visible = shouldRender;

            prevElement = element;

            index++;
        }
    }
}