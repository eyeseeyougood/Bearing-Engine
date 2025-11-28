using System.Drawing;
using Silk.NET.Input;
using OpenTK.Mathematics;

namespace Bearing;

public class UIElement : MeshRenderer
{
    public UITheme theme = UIManager.currentTheme;
    public UITheme themeOverride;

    public UIElement() : base("NONE", false, true) { themeOverride = (UITheme)UITheme.Empty.Clone(); UIManager.AddUI(this); setup3DMatrices = false; SetMesh(UIManager.quadMeshCache); }

    public List<string> consumedInputs = new List<string>() { "mouseEnter" }; // this really needs a better way to standardise this

    public int renderLayer { get; set; }

    protected bool _setVisible = true;
    protected bool _visible = true;
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
            if (!visible)
            {
                if (mouseOver)
                {
                    UIManager.SendEvent(this, "MouseExit");
                }
            }
        }
    }

    public Action positionChanged = () => { };
    public Action sizeChanged = () => { };
    public Action onCleanup = () => { };

    protected UIElement _parent;

    protected bool mouseOver = false;

    public int parent { // id
        get
        {
            if (_parent != null)
                return _parent.rid;

            return -1;
        }
        set
        {
            if (_parent != null)
            {
                _parent.positionChanged -= UpdatePosition;
                _parent.sizeChanged -= UpdateSize;
                _parent.onCleanup -= ParentCleanedUp;
            }

            _parent = UIManager.FindFromRID(value);

            if (_parent != null)
            {
                _parent.positionChanged += UpdatePosition;
                _parent.sizeChanged += UpdateSize;
                _parent.onCleanup += ParentCleanedUp;
            }

            UpdatePosition();
        }
    }

    protected Vector2 _anchor { get; set; } = new Vector2(0,0);
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

    protected UDim2 _setPos{ get; set; } = new UDim2(0.0f, 0.0f, 0, 0);
    protected UDim2 _position { get; set; } = new UDim2(0.0f, 0.0f, 0, 0);
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
    protected UDim2 _setSize { get; set; } = new UDim2(0.0f,0.0f,200,200);
    protected UDim2 _size { get; set; } = new UDim2(0.0f,0.0f,200,200);
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

    public T GetThemeValue<T>(string key)
    {
        if (typeof(UITheme).GetField(key).GetValue(themeOverride) == null)
        {
            return (T)typeof(UITheme).GetField(key).GetValue(theme);
        }
        else
        {
            return (T)typeof(UITheme).GetField(key).GetValue(themeOverride);
        }
    }

    public Vector4 GetScreenBoundingBox()
    {
        Vector2 sizing = size.scale * Game.instance.ClientSize + size.offset;
        Vector2 positioning = position.scale * Game.instance.ClientSize + position.offset + sizing / 2.0f - anchor * sizing;

        Vector2 p1 = new Vector2(-0.5f, -0.5f) * sizing + positioning;
        Vector2 p2 = new Vector2(0.5f, 0.5f) * sizing + positioning;

        return new Vector4((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y);
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

        float screenW = Game.instance.ClientSize.X;
        float screenH = Game.instance.ClientSize.Y;

        if (!visible)
            return;

        // TODO: OPTIMISATION - getting bounds box
        bool m = Extensions.PointInQuad(Input.GetMousePosition(), GetScreenBoundingBox());
        if (m && !mouseOver && consumedInputs.Contains("mouseEnter"))
        {
            // mouse entered
            UIManager.SendEvent(this, "MouseEnter");
        }
        else if (!m && mouseOver)
        {
            // mouse left
            UIManager.SendEvent(this, "MouseExit");
        }

        mouseOver = m;
    }

    public void ParentCleanedUp()
    {
        gameObject.RemoveComponent(this, false);

        Cleanup();
    }

    public override void Cleanup()
    {
        onCleanup.Invoke();
        base.Cleanup();

        if (_parent != null)
        {
            _parent.positionChanged -= UpdatePosition;
            _parent.sizeChanged -= UpdateSize;
            _parent.onCleanup -= ParentCleanedUp;
        }

        if (mouseOver)
        {
            UIManager.SendEvent(this, "MouseExit");
        }

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
    public UIPanel() : base()
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
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter(new ShaderParam("mainColour", GetThemeValue<BearingColour>("uiPanelBG").zeroToOne));
    }
}

public class UIImage : UIElement
{
    public UIImage() : base()
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
                new ShaderParam() { name = "mainColour", vector4 = new Vector4(0.9f, 0.9f, 0.9f, 1.0f) },
            },
            is3D = false
        };
    }

    public void SetTexture(Texture texture)
    {
        if (texture0 != null)
        {
            texture0.Dispose();
        }

        texture0 = texture;
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter(new ShaderParam("mainColour", Vector4.One));
        material.SetShaderParameter(new ShaderParam("texSize", new Vector2(texture0._width, texture0._height)));
        material.SetShaderParameter(new ShaderParam("fitToTexRatio", 0));
    }
}

public class UILabel : UIElement
{
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

    public UILabel() : base() { }

    public override void Cleanup()
    {
        if (onTextChanged != null)
        {
            Delegate[] subscribers = onTextChanged.GetInvocationList();
            foreach (var d in subscribers)
                onTextChanged -= d as EventHandler<string>;
        }
        if (texture0 != null)
        {
            texture0.Dispose();
        }
        base.Cleanup();
    }

    public override void OnLoad()
    {
        material = new Material()
        {
            shader = new Shader("defaultUI.vert", "textUI.frag"),
            attribs = new List<ShaderAttrib>()
            {
                new ShaderAttrib() { name = "aPosition", size = 2 },
                new ShaderAttrib() { name = "aTexCoord", size = 2 },
            },
            is3D = false
        };

        ResetTexture();

        base.OnLoad();
    }

    public virtual void SetTextWithoutEventTrigger(string newValue)
    {
        _text = newValue;

        ResetTexture();
    }

    protected virtual void ResetTexture()
    {
        if (texture0 != null)
        {
            texture0.Dispose();
        }

        texture0 = UIManager.UITextHelper.RenderTextToBmp(text, font);
    }

    protected virtual void TextChanged(string val)
    {
        if (onTextChanged != null)
            onTextChanged.Invoke(this, val);

        ResetTexture();
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter(new ShaderParam("mainColour", theme.labelText.Value.zeroToOne));
        material.SetShaderParameter(new ShaderParam("texSize", new Vector2(texture0._width, texture0._height)));
        fitHeightToWidth = true;
        material.SetShaderParameter(new ShaderParam("fitToTexRatio", fitHeightToWidth ? 1:0));
    }
}

public class UITextBox : UILabel
{
    private UIButton button;

    private bool selected = false;

    public bool multiline { get; set; } = true;

    public event EventHandler<string> onTextSubmit = (i, j) => { };

    private bool emptyText = false;

    private int caretPos;
    private int caretLine;

    public UITextBox() : base()
    {
        consumedInputs = new List<string>()
        {
            "leftClick",
            "enter",
            "characters",
            "escape",
            "leftShift",
            "backspace",
            "leftArrow",
            "rightArrow",
            "upArrow",
            "downArrow",
        };
    }

    public void ClearSubmitEventSubscribers()
    {
        foreach (var d in onTextSubmit.GetInvocationList())
        {
            onTextSubmit -= (EventHandler<string>)d;
        }
    }

    protected override void ResetTexture()
    {
        base.ResetTexture();
    }

    public override void Cleanup()
    {
        if (onTextSubmit != null)
        {
            Delegate[] subscribers = onTextSubmit.GetInvocationList();
            foreach (var d in subscribers)
                onTextSubmit -= d as EventHandler<string>;
        }

        button.buttonPressed -= Pressed;
        UIManager.uiEvent -= OnEvent;

        Input.onCharacterPressed -= OnCharacterPressed;

        base.Cleanup();
    }

    public override void SetTextWithoutEventTrigger(string newValue)
    {
        if (newValue == " ")
        {
            emptyText = true;
        }
        base.SetTextWithoutEventTrigger(newValue);
        CaretToEnd();
        
    }

    public void CaretToEnd()
    {
        if (!emptyText)
        {
            caretPos = text.Split("\n").Last().Length;
            caretLine = text.Split("\n").Length - 1;
        }
        else
        {
            caretPos = 0;
            caretLine = 0;
        }
    }

    public override void OnLoad()
    {
        button = new UIButton()
        {
            parent = rid,
            renderLayer = renderLayer-1,

            theme = theme,

            anchor = new Vector2(0.5f, 0.5f),

            position = new UDim2(0.5f, 0.5f),

            size = new UDim2(1, 1),
        };

        gameObject.AddComponent(button);

        button.buttonPressed += Pressed;
        UIManager.uiEvent += OnEvent;

        base.OnLoad();

        Input.onCharacterPressed += OnCharacterPressed;
    }

    private void OnEvent(object? sender, string e)
    {
        if (e != "UIClicked")
            return;

        if (sender != button && selected)
        {
            Deselect();
            onTextSubmit.Invoke(this, text);
        }
    }

    public void Deselect()
    {
        selected = false;
    }

    public void Select()
    {
        selected = true;
        CaretToEnd();
    }

    private void Pressed(object? sender, EventArgs e)
    {
        Select();
    }

    private int LenOfCurLine()
    {
        return text.Split("\n")[caretLine].Length;
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        button.themeOverride.buttonHoverBackground = selected ? theme.selection : null;
        button.themeOverride.buttonDownBackground = selected ? theme.selection : null;
        button.themeOverride.buttonUpBackground = selected ? theme.selection : null;

        if (Input.GetKeyDown(Key.Backspace) && selected)
        {
            if (caretPos > 0 || caretLine > 0)
            {
                // TODO: OPTIMISATION - sums all chars on every line until the caret
                text = text.Remove(caretPos+SumOfLineChars(caretLine)-1, 1);
                caretPos--;
                if (caretPos < 0 && caretLine == 0) caretPos = 0;
                else if (caretPos < 0) { caretLine--; caretPos = LenOfCurLine(); }
                if (text == "")
                {
                    text = " ";
                    emptyText = true;
                }
            }
        }
        if ((Input.GetKeyDown(Key.Enter)||Input.GetKeyDown(Key.KeypadEnter)) && Input.GetKey(Key.ShiftLeft) && selected && multiline)
        {
            text += "\n";
            caretLine++;
            caretPos = 0;
        }
        else if ((Input.GetKeyDown(Key.Escape) || Input.GetKeyDown(Key.Enter) || Input.GetKeyDown(Key.KeypadEnter)) && selected)
        {
            Deselect();
            onTextSubmit.Invoke(this, text);
        }

        if (Input.GetKeyDown(Key.Left) && selected)
        {
            caretPos--;
            if (caretPos < 0)
            {
                if (caretLine > 0)
                {
                    caretLine--;
                    caretPos = LenOfCurLine();
                }
                else
                {
                    caretPos = 0;
                }
            }
        }
        if (Input.GetKeyDown(Key.Right) && selected)
        {
            caretPos++;
            if (caretPos > text.Split("\n")[caretLine].Length)
            {
                if (caretLine+1 < text.Split("\n").Length)
                {
                    caretLine++;
                    caretPos = 0;
                }
                else
                {
                    caretPos = text.Split("\n")[caretLine].Length;
                }
            }
        }

        if (Input.GetKeyDown(Key.Up) && selected)
        {
            caretLine--;
            if (caretLine < 0) caretLine = 0;

            if (caretPos > LenOfCurLine()) caretPos = LenOfCurLine();
        }

        if (Input.GetKeyDown(Key.Down) && selected)
        {
            caretLine++;
            if (caretLine >= text.Split("\n").Length) caretLine = text.Split("\n").Length-1;

            if (caretPos > LenOfCurLine()) caretPos = LenOfCurLine();
        }

        string[] lines = text.Split("\n");
        string currLine = lines[caretLine];
        string preText = currLine.Substring(0, caretPos);
        material.SetShaderParameter(new ShaderParam("caretPos", new Vector2(UIManager.UITextHelper.MeasureText(preText, font), caretLine * UIManager.UITextHelper.fontHeights[font])));
        material.SetShaderParameter(new ShaderParam("caretSize", new Vector2(2, UIManager.UITextHelper.fontHeights[font]) * (selected?1:0)));
    }

    private int SumOfLineChars(int n)
    {
        int sumOfLines = 0;
        int idx = 0;
        foreach (string s in text.Split("\n", StringSplitOptions.None))
        {
            if (idx >= n)
            {
                break;
            }
            sumOfLines += s.Length + 1;
            idx++;
        }

        return sumOfLines;
    }

    private void OnCharacterPressed(string character)
    {
        if (!selected)
            return;

        if (emptyText)
        {
            text += character[0];
            text = string.Join("", text.Skip(1));
            emptyText = false;
        }
        else
        {
            // TODO: OPTIMISATION - sums all chars on every line until the caret
            text = text.Insert(caretPos+SumOfLineChars(caretLine), character[0].ToString());
        }

        caretPos++;

        ResetTexture();
    }
}

public class UIButton : UIElement
{
    public event EventHandler buttonPressed = (i, j) => { };
    public event EventHandler buttonHold = (i, j) => { };
    public event EventHandler buttonReleased = (i, j) => { };

    public event EventHandler mouseEnter = (i, j) => { };
    public event EventHandler mouseLeave = (i, j) => { };

    public UIButton() : base()
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
    }

    private bool prevHover = false;
    private bool hover = false;
    private bool prevPressed = false;
    private bool pressed = false;

    private void RemoveSubscribers(EventHandler del)
    {
        if (del != null)
        {
            Delegate[] subscribers = del.GetInvocationList();
            foreach (var d in subscribers)
                del -= d as EventHandler;
        }
    }

    public override void Cleanup()
    {
        RemoveSubscribers(buttonPressed);
        RemoveSubscribers(buttonHold);
        RemoveSubscribers(buttonReleased);
        RemoveSubscribers(mouseEnter);
        RemoveSubscribers(mouseLeave);

        base.Cleanup();
    }

    private bool mouseDown;
    private bool prevMouseDown;
    private bool wasPressed;
    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        BearingColour bg = GetThemeValue<BearingColour>("buttonUpBackground");

        mouseDown = Input.GetMouseButton(0);

        pressed = false;
        hover = false;
        if (mouseOver && visible && (UIManager.mouseUsingObject == this || UIManager.mouseUsingObject == null))
        {
            hover = true;
            bg = GetThemeValue<BearingColour>("buttonHoverBackground");
            if (Input.GetMouseButton(0))
            {
                bg = GetThemeValue<BearingColour>("buttonDownBackground");
                pressed = true;
            }
        }

        if (hover && !prevHover)
        {
            // mouse entered this frame
            mouseEnter.Invoke(this, new EventArgs());

            var tg = GetThemeValue<string>("buttonHoverAudio");

            if (GetThemeValue<string>("buttonHoverAudio") != "None")
            {
                UIManager.PlaySFX(Resource.GetSFX(GetThemeValue<string>("buttonHoverAudio"), true));
            }
        }

        if (!hover && prevHover)
        {
            // mouse left this frame
            mouseLeave.Invoke(this, new EventArgs());
        }

        if (pressed && !prevPressed)
        {
            // pressed this frame
            buttonPressed.Invoke(this, new EventArgs());
            wasPressed = true;
            UIManager.mouseUsingObject = this;

            UIManager.SendEvent(this, "UIClicked");

            if (GetThemeValue<string>("buttonDownAudio") != "None")
                UIManager.PlaySFX(Resource.GetSFX(GetThemeValue<string>("buttonDownAudio"), true));
        }

        if (pressed)
        {
            // call hold
            buttonHold.Invoke(this, new EventArgs());
        }

        if (!mouseDown && prevMouseDown && wasPressed)
        {
            // released this frame
            buttonReleased.Invoke(this, new EventArgs());
            wasPressed = false;
            UIManager.mouseUsingObject = null;

            if (GetThemeValue<string>("buttonUpAudio") != "None")
                UIManager.PlaySFX(Resource.GetSFX(GetThemeValue<string>("buttonUpAudio"), true));
        }

        prevPressed = pressed;
        prevHover = hover;
        prevMouseDown = mouseDown;

        // handle colour
        material.SetShaderParameter(new ShaderParam("mainColour", bg.zeroToOne));
    }
}

/// <summary>
/// Simple Slider for getting a value from 0 to 1.
///
/// Uses 2 RenderLayers above own
/// </summary>
public class UIVerticalSlider : UIPanel
{
    private UIPanel fill;
    private UIButton button;
    private bool dragging = false;
    public bool showFillLine = true;
    public float value = 0.5f;

    public override void OnLoad()
    {
        base.OnLoad();

        themeOverride.uiPanelBG = UIManager.currentTheme.sliderBackground;

        button = new UIButton();
        button.parent = rid;
        button.anchor = new Vector2(0.5f,0.5f);
        button.position = new UDim2(0.5f,0.5f);
        button.size = new UDim2(1f,0,10,10);
        button.renderLayer = renderLayer + 2;
        button.buttonPressed += ButtonClicked;
        gameObject.AddComponent(button);

        fill = new UIPanel();
        fill.parent = rid;
        fill.anchor = new Vector2(0.5f, 1.0f);
        fill.position = new UDim2(0.5f, 1.0f);
        fill.size = new UDim2(1f, 0.5f);
        fill.renderLayer = renderLayer + 1;
        fill.themeOverride.uiPanelBG = UIManager.currentTheme.sliderFill;
        gameObject.AddComponent(fill);
    }

    public void ButtonClicked(object? sender, EventArgs e)
    {
        dragging = true;
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        fill.visible = showFillLine;
        fill.size = new UDim2(1f, value);

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (!dragging)
            return;

        // TODO:
        // this maths doesnt work when the size offset is changed, so dont use size offset on this object until i figure out how to neatly normalise UDims with propagation
        var mouseRatio = (Input.GetMousePosition().Y-position.offset.Y) / (Game.instance.ClientSize.Y);
        var sliderTopRatio = position.scale.Y- (anchor.Y * size.scale.Y);
        var sliderBottomRatio = sliderTopRatio + size.scale.Y;
        var percent = (mouseRatio - sliderTopRatio) / (sliderBottomRatio - sliderTopRatio);
        percent = Math.Clamp(percent, 0, 1);
        value = 1f-percent;
        button.position = new UDim2(0.5f, percent);
    }

    public override void Cleanup()
    {
        base.Cleanup();

        button.Cleanup();
        fill.Cleanup();
    }
}

public class UIVerticalScrollView : UIElement
{
    public int scrollSensitivity { get; set; } = 1;
    public int spacing { get; set; } = 5;
    public bool clipContents { get; set; } = true;
    public bool scrollByComponents { get; set; } = true;

    public List<int> contents { get; set; } = new List<int>();

    private int scroll;
    private float scrollOffset;

    public UIVerticalScrollView() : base()
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

        consumedInputs.Add("Scroll");
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        if (!ChildAbsorbedScroll() && MathF.Abs(Input.GetMouseScrollDelta().Y) > 0)
            if (mouseOver && visible)
            {
                int delta = (int)Input.GetMouseScrollDelta().Y;

                int numInvis = GetInvisibleElements();

                scroll += delta * scrollSensitivity;
                
                if (scrollByComponents)
                {
                    if (-scroll >= contents.Count || -scroll < 0 || (numInvis == 0 && clipContents))
                        scroll -= delta * scrollSensitivity;
                }
                else
                {
                    if (-scroll >= GetNormalisedSumOfHeights() * Game.instance.ClientSize.Y || -scroll < 0)
                        scroll -= delta * scrollSensitivity;
                }
            }

        material.SetShaderParameter(new ShaderParam("mainColour", GetThemeValue<BearingColour>("verticalScrollBG").zeroToOne));
    }

    private bool ChildAbsorbedScroll()
    {
        bool result = false;

        foreach (int el in contents)
        {
            UIElement elem = UIManager.FindFromRID(el);

            if (elem == null)
            {
                continue;
            }

            if (!elem.consumedInputs.Contains("Scroll"))
                continue;

            if (Extensions.PointInQuad(Input.GetMousePosition(), elem.GetScreenBoundingBox()))
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
            UIElement element = UIManager.FindFromRID(elem);

            if (element != null)
            {
                element.gameObject.RemoveComponent(element);
            }
        }

        contents.Clear();
    }

    public override void Cleanup()
    {
        ClearContents();

        base.Cleanup();
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
            UIElement? element = UIManager.FindFromRID(item);

            if (element != null)
                element.visible = false;
        }
    }

    // TODO: OPTIMISATION - can make this better by just keeping a cache variable that increments and decrements from the beforerender function
    private int GetInvisibleElements()
    {
        int result = 0;

        foreach (int item in contents)
        {
            UIElement? element = UIManager.FindFromRID(item);
            if (element == null) { continue; }

            if (!element.visible)
            {
                result++;
            }
        }

        return result;
    }

    private float GetNormalisedSumOfHeights()
    {
        float result = 0;
        for (int i = 0; i < contents.Count; i++)
        {
            UIElement el = UIManager.FindFromRID(contents[i]);
            if (el == null) { continue; }

            Vector2 normalisedScale = el.size.scale + (el.size.offset / Game.instance.ClientSize);
            result += normalisedScale.Y;
        }

        result += (spacing / (float)Game.instance.ClientSize.Y) * (contents.Count-1);

        return result;
    }

    private void ScrollOffsetByComponents()
    {
        scrollOffset = 0;
        for (int i = 0; i < -scroll; i++)
        {
            UIElement el = UIManager.FindFromRID(contents[i]);
            if (el == null) { continue; }

            Vector2 normalisedScale = el.size.scale + (el.size.offset / Game.instance.ClientSize);
            scrollOffset += normalisedScale.Y;
        }

        scrollOffset += spacing / (float)Game.instance.ClientSize.Y * -scroll;
    }

    private void ScrollOffsetByPixels()
    {
        scrollOffset = -scroll * scrollSensitivity;
        scrollOffset /= Game.instance.ClientSize.Y;
    }

    private void UpdateScrollOffset()
    {
        if (scrollByComponents)
            ScrollOffsetByComponents();
        else
            ScrollOffsetByPixels();
    }

    protected override void BeforeRender()
    {
        base.BeforeRender();

        UpdateScrollOffset();

        int index = 0;
        UIElement prevElement = null;
        foreach (int item in contents)
        {
            UIElement? element = UIManager.FindFromRID(item);

            if (element == null) { continue; }

            float newIndex = index + scroll;

            Vector2 normalisedScale = size.scale + (size.offset / Game.instance.ClientSize);
            Vector2 elementNormalisedScale = element.size.scale + (element.size.offset / Game.instance.ClientSize);
            Vector2 prevElementNormalisedScale = elementNormalisedScale;

            Vector2 prevElementOffsetPos = new Vector2(0, -scrollOffset - elementNormalisedScale.Y) + (position.offset-Vector2.UnitY*spacing) / Game.instance.ClientSize;
            if (prevElement != null)
            {
                prevElementNormalisedScale = prevElement.size.scale + (prevElement.size.offset / Game.instance.ClientSize);
                prevElementOffsetPos = prevElement.position.offset / Game.instance.ClientSize;
            }

            float elementOffset = prevElementOffsetPos.Y * Game.instance.ClientSize.Y + prevElementNormalisedScale.Y * Game.instance.ClientSize.Y;

            element.position = new UDim2(position.scale, new Vector2(0, spacing + elementOffset));
            element.size = new UDim2(new Vector2(size.scale.X, element.size.scale.Y), element.size.offset + new Vector2(size.offset.X, 0));

            element.anchor = anchor;

            // check if still in bounding box otherwise dont render

            Vector4 ebb = element.GetScreenBoundingBox();

            Vector4 obb = GetScreenBoundingBox();

            bool shouldRender = true;
            
            if (clipContents)
            {
                if (!Extensions.PointInQuad(new Vector2(ebb.X, ebb.Y), obb))
                    shouldRender = false;
                else if (!Extensions.PointInQuad(new Vector2(ebb.Z, ebb.W), obb))
                    shouldRender = false;
            }

            element.visible = shouldRender;

            prevElement = element;

            index++;
        }
    }

    // TODO: IMPLEMENT (cant currently cus scroll view doesnt have good bounds)
    public class UIVerticalScrollBar : UIPanel
    {
        private UIPanel bar;

        protected override void BeforeRender()
        {
            if (parent == -1) return;

            // TODO: OPTIMISATION - difficulty:1

            // TODO: for now this assumes parent is uiverticalscrollview, but later once more vertical scrolling ui exists, add an IScrollable interface
            UIVerticalScrollView par = (UIVerticalScrollView)UIManager.FindFromRID(parent);

            bar.position = par.position;
            bar.anchor = par.anchor;
            bar.size = new UDim2(par.size.scale * new Vector2(1,0.5f), par.size.offset);
            base.BeforeRender();
        }
    }
}