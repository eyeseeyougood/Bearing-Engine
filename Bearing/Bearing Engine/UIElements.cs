using System.Drawing;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing;

public enum UIMouseCaptureMode
{
    PassThrough = 0,
    HandleAndPass = 1,
    Consume = 2,
}

[DontSerialise]
public class UIElement : SpriteRenderer
{
    public UITheme theme = UIManager.currentTheme;
    public UITheme themeOverride;

    public UIElement(params object[] meta) : base() { themeOverride = (UITheme)UITheme.Empty.Clone(); UIManager.AddUI(this); metadata = meta; }

    public UIMouseCaptureMode mouseCaptureMode = UIMouseCaptureMode.Consume;

    public int renderLayer { get; set; }

    public bool useParentVisibility = true;
    public bool useParentActivity = true;

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
            _visible = value;
        }
    }

    protected bool _setActive = true;
    protected bool _active = true;
    public bool active
    {
        get
        {
            return _active;
        }
        set
        {
            _setActive = value;
            _active = value;

            if (!_active && mouseOver)
            {
                UIManager.SendEvent(this, "MouseExit");
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
            return _setPos;
        }
        set {
            _setPos = value;
            UpdatePosition();
            positionChanged.Invoke();
        }
    }

    public UDim2 worldPosition { 
        get {
            return _position;
        }
    }

    protected UDim2 _setSize { get; set; } = new UDim2(0.0f,0.0f,200,200);
    protected UDim2 _size { get; set; } = new UDim2(0.0f,0.0f,200,200);
    public UDim2 size
    {
        get
        {
            return _setSize;
        }
        set
        {
            _setSize = value;
            UpdateSize();
            sizeChanged.Invoke();
        }
    }

    public UDim2 worldSize
    {
        get
        {
            return _size;
        }
    }

    public T? GetThemeValue<T>(string key)
    {
        if (typeof(T) == typeof(BearingColour))
        {
            return (T?)(object?)(themeOverride.ContainsColour(key) ? themeOverride.GetColour(key) : theme.GetColour(key));
        }
        else if (typeof(T) == typeof(Resource))
        {
            return (T?)(object?)(themeOverride.ContainsAudio(key) ? themeOverride.GetAudio(key) : theme.GetAudio(key));
        }
        else
        {
            throw new Exception("You are either being silly or doing some unimaginable levels of voodoo");
        }
    }

    public Vector4 GetScreenBoundingBox()
    {
        Vector2 sizing = worldSize.scale * Game.instance.ClientSize + worldSize.offset;
        Vector2 positioning = worldPosition.scale * Game.instance.ClientSize + worldPosition.offset + sizing / 2.0f - anchor * sizing;

        Vector2 p1 = new Vector2(-0.5f, -0.5f) * sizing + positioning;
        Vector2 p2 = new Vector2(0.5f, 0.5f) * sizing + positioning;

        return new Vector4((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y);
    }

    public override string ToString()
    {
        return $"UIElement ({GetType().FullName}) - meta: {metadata.ElementsToString()}";
    }

    public bool GetSetVisibility()
    {
        return _setVisible;
    }

    public void SetVisibility(bool visible, bool changeSetVisibility = true)
    {
        if (changeSetVisibility)
            this.visible = visible;
        else
            _visible = visible;
    }

    public bool GetSetActive()
    {
        return _setActive;
    }

    public void SetActive(bool active, bool changeSetActive = true)
    {
        if (changeSetActive)
            this.active = active;
        else
            _active = active;
    }

    protected virtual void UpdateVisibility()
    {
        if (!_setVisible)
        {
            _visible = false;
            return;
        }

        if (useParentVisibility && _parent != null)
        {
            _visible = _parent.visible;
        }
    }

    /// <summary>
    /// Force a position recalculation
    /// </summary>
    public void UpdatePosition()
    {
        if (_parent == null) { _position = _setPos; return; }

        Vector2 parentNormalisedScale = _parent.worldSize.Normalize(Game.instance.ClientSize);

        Vector2 scale = _setPos.scale
                      * parentNormalisedScale
                      + _parent.worldPosition.scale
                      - _parent.anchor
                      * parentNormalisedScale;

        _position = new UDim2(scale, _setPos.offset + _parent.worldPosition.offset);
    }

    /// <summary>
    /// Force a size recalculation
    /// </summary>
    public void UpdateSize()
    {
        if (_parent == null) { _size = _setSize; return; }

        _size = new UDim2(_setSize.scale * (_parent.worldSize.Normalize(Game.instance.ClientSize)), _setSize.offset);
    }

    public void UpdateActive()
    {
        if (!useParentActivity)
        {
            return;
        }

        if (!_setActive)
        {
            _active = false;
        }

        if (_parent == null)
        {
            _active = true;
            return;
        }

        _active = _parent.active;
    }

    public override void OnLoad()
    {
        base.OnLoad();

        Game.instance.RemoveRenderable(this); // ui should not be handled like all other renderables XDD
    }

    public override void OnTick(float dt)
    {
        UpdatePosition();
        UpdateSize();
        UpdateActive();

        float screenW = Game.instance.ClientSize.X;
        float screenH = Game.instance.ClientSize.Y;

        if (!active)
            return;

        // TODO: OPTIMISATION - getting bounds box
        bool m = Extensions.PointInQuad(Input.GetMousePosition(), GetScreenBoundingBox());
        if (m && !mouseOver && mouseCaptureMode != UIMouseCaptureMode.PassThrough)
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

        Texture? t = sprite.Peak();

        if (t != null)
            t.Use(TextureUnit.Texture0);

        BeforeRender();

        material.Use();

        GL.DrawElements(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    public void OnMouseEvent()
    {
        switch (mouseCaptureMode)
        {
            case UIMouseCaptureMode.PassThrough:
                if (_parent is not null)
                    _parent.OnMouseEvent();
                break;
            case UIMouseCaptureMode.HandleAndPass:
                OnHandleMouseEvent();
                if (_parent is not null)
                    _parent.OnMouseEvent();
                break;
            case UIMouseCaptureMode.Consume:
                OnHandleMouseEvent();
                break;
        }
    }

    protected virtual void OnHandleMouseEvent() {}

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
}

public class UIPanel : UIElement
{
    public UIPanel(params object[] meta) : base(meta)
    {
        material = new Material()
        {
            shader = new Shader("eng/defaultUI.vert", "eng/defaultUI.frag"),
        };
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter("mainColour", GetThemeValue<BearingColour>("panelBG").zeroToOne);
    }
}

public class UIImage : UIElement
{
    public UIImage(params object[] meta) : base(meta)
    {
        material = new Material()
        {
            shader = new Shader("eng/defaultUI.vert", "eng/textureUI.frag"),
            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", value = new List<object> {1.0f, 1.0f, 1.0f, 1.0f} },
            },
        };
    }

    public void SetTexture(Texture texture, bool cleanupTextures = true)
    {
        sprite.SetTexture(texture, cleanupTextures);
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter("mainColour", Vector4.One);
        material.SetShaderParameter("fitToTexRatio", 0);

        Texture tex = sprite.Peak();

        if (tex != null)
            material.SetShaderParameter("texSize", new Vector2(tex._width, tex._height));
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

    private int _truncateThreshold = -1;
    public int truncateThreshold {
        get {
            return _truncateThreshold;
        }
        set {
            _truncateThreshold = value;
            ResetTexture();
        }
    }
    public bool useElipsisTruncation { get; set; } = false;

    public event EventHandler<string> onTextChanged = (i,j) => { };

    public UILabel(params object[] meta) : base(meta) { }

    public override void Cleanup()
    {
        if (onTextChanged != null)
        {
            Delegate[] subscribers = onTextChanged.GetInvocationList();
            foreach (var d in subscribers)
                onTextChanged -= d as EventHandler<string>;
        }
        base.Cleanup();
    }

    public override void OnLoad()
    {
        material = new Material()
        {
            shader = new Shader("eng/defaultUI.vert", "eng/textUI.frag"),
        };

        ResetTexture();

        base.OnLoad();
    }

    public virtual void SetTextWithoutEventTrigger(string newValue)
    {
        _text = newValue;

        ResetTexture();
    }

    public virtual void ResetTexture()
    {
        string finalText = text;

        if (text == "")
            finalText = " ";

        if (text.Length > truncateThreshold && truncateThreshold != -1)
        {
            finalText = finalText.Substring(0, truncateThreshold);

            if (useElipsisTruncation)
                finalText += "...";
        }

        sprite.SetTexture(UIManager.UITextHelper.RenderTextToBmp(finalText, font));
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

        Texture? tex = sprite.Peak();

        material.SetShaderParameter("mainColour", GetThemeValue<BearingColour>("labelText").zeroToOne);
        if (tex is not null)
            material.SetShaderParameter("texSize", new Vector2(tex._width, tex._height));
        fitHeightToWidth = true;
        material.SetShaderParameter("fitToTexRatio", fitHeightToWidth ? 1:0);
    }
}

/*
public class UITextBox : UILabel
{
    protected UIButton button;

    protected bool selected = false;

    public bool multiline { get; set; } = true;

    public event EventHandler<string> onTextSubmit = (i, j) => { };
    public event Action<UITextBox> onPressed = (i) => { };

    protected bool emptyText = false;

    protected int caretPos;
    protected int caretLine;

    public UITextBox(params object[] meta) : base(meta) {}

    public void ClearText()
    {
        emptyText = true;
        text = " ";
        caretLine = 0;
        caretPos = 0;
        ResetTexture();
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

            useParentActivity = false,
        };

        gameObject.AddComponent(button);

        active = false;

        button.buttonPressed += Pressed;
        UIManager.uiEvent += OnEvent;

        base.OnLoad();

        Input.onCharacterPressed += OnCharacterPressed;
    }

    protected void OnEvent(object? sender, string e)
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

    protected void Pressed(UIButton sender)
    {
        Select();
        onPressed.Invoke(this);
    }

    protected int LenOfCurLine()
    {
        return text.Split("\n")[caretLine].Length;
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        button.themeOverride.SetColour("buttonHoverBackground", selected ? theme.GetColour("selection") : null);
        button.themeOverride.SetColour("buttonDownBackground", selected ? theme.GetColour("selection") : null);
        button.themeOverride.SetColour("buttonUpBackground", selected ? theme.GetColour("selection") : null);

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
        material.SetShaderParameter("caretPos", new Vector2(UIManager.UITextHelper.MeasureText(preText, font), caretLine * UIManager.UITextHelper.fontHeights[font]));
        material.SetShaderParameter("caretSize", new Vector2(2, UIManager.UITextHelper.fontHeights[font]) * (selected?1:0));
    }

    protected int SumOfLineChars(int n)
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

    protected void OnCharacterPressed(string character)
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
}*/

///<summary>
///!!! Consumes 2 Renderlayers !!!
///</summary>
public class UITextBox : UIButton
{
    public UITextBox(params object[] meta) : base(meta) {}

    public event Action<UITextBox> textSubmitted = (i) => {};

    private string _placeholderText = "enter text here...";
    public string placeholderText {
        get {
            return _placeholderText;
        }
        set {
            _placeholderText = value;
            if (label is not null)
                Deselect();
        }
    }
    private string _text = "";
    public string text {
        get {
            return _text;
        }
        set {
            _text = value;
            if (label is not null)
                Deselect();
        }
    }

    public int truncateThreshold {
        get {
            return label.truncateThreshold;
        }
        set {
            label.truncateThreshold = value;
        }
    }

    public bool useElipsisTruncation {
        get {
            return label.useElipsisTruncation;
        }
        set {
            label.useElipsisTruncation = value;
        }
    }

    protected bool selected;

    public bool isSelected
    {
        get {
            return selected;
        }
        set {
            selected = value;
        }
    }

    protected BearingColour? themeSave1;
    protected BearingColour? themeSave2;
    protected BearingColour? themeSave3;

    protected UILabel label;

    public override void OnLoad()
    {
        base.OnLoad();

        buttonPressed += Pressed;

        Input.onCharacterPressed += onCharacterPressed;

        label = new UILabel();
        label.parent = rid;
        label.renderLayer = renderLayer + 1;
        label.position = new UDim2(0,0,10,10);
        label.size = new UDim2(1,1,-20,-20);
        label.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
        gameObject.AddComponent(label);

        Deselect();
    }

    public void ResetTexture()
    {
        label.ResetTexture();
    }

    protected virtual void Pressed(UIButton sender)
    {
        Select();
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        if (!selected)
            return;

        if (Input.GetKeyDown(Key.Escape))
        {
            Deselect();
        }

        if (Input.GetKeyDown(Key.Enter) || Input.GetKeyDown(Key.KeypadEnter))
        {
            Deselect();
            textSubmitted.Invoke(this);
        }

        if (Input.GetKeyDown(Key.Backspace))
        {
            RemoveCharacter();
        }
    }

    protected virtual void onCharacterPressed(string s)
    {
        if (!selected)
            return;
        
        _text += s[0];
        label.text = text;
    }

    public void RemoveCharacter()
    {
        if (text.Length - 1 < 0)
            return;

        _text = text.Substring(0, text.Length - 1);

        label.text = text;
    }

    public void Deselect()
    {
        if (selected)
        {
            themeOverride.SetColour("buttonHoverBackground", themeSave1);
            themeOverride.SetColour("buttonUpBackground", themeSave2);
            themeOverride.SetColour("buttonDownBackground", themeSave3);
        }

        selected = false;

        if (text == "")
            label.text = placeholderText;
    }

    public void Select()
    {
        if (!selected)
        {
            themeSave1 = themeOverride.GetColour("buttonHoverBackground");
            themeSave2 = themeOverride.GetColour("buttonUpBackground");
            themeSave3 = themeOverride.GetColour("buttonDownBackground");

            themeOverride.SetColour("buttonHoverBackground", GetThemeValue<BearingColour>("selection"));
            themeOverride.SetColour("buttonUpBackground", GetThemeValue<BearingColour>("selection"));
            themeOverride.SetColour("buttonDownBackground", GetThemeValue<BearingColour>("selection"));
        }

        selected = true;
    }
}

public class UIButton : UIElement
{
    public event Action<UIButton> buttonPressed = (i) => { };
    public event Action<UIButton> buttonHold = (i) => { };
    public event Action<UIButton> buttonReleased = (i) => { };

    public event Action<UIButton> mouseEnter = (i) => { };
    public event Action<UIButton> mouseLeave = (i) => { };

    public UIButton(params object[] meta) : base(meta)
    {
        material = new Material()
        {
            shader = new Shader("eng/defaultUI.vert", "eng/defaultUI.frag"),
        };
    }

    private bool prevHovered = false;
    private bool pressed = false;

    private void RemoveSubscribers(Action<UIButton> del)
    {
        if (del != null)
        {
            Delegate[] subscribers = del.GetInvocationList();
            foreach (var d in subscribers)
                del -= d as Action<UIButton>;
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
    /*
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

            if (GetThemeValue<Resource>("buttonHoverAudio") is not null)
                UIManager.PlaySFX(GetThemeValue<Resource>("buttonHoverAudio"));
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

            if (GetThemeValue<Resource>("buttonDownAudio") is not null)
                UIManager.PlaySFX(GetThemeValue<Resource>("buttonDownAudio"));
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

            if (GetThemeValue<Resource>("buttonUpAudio") is not null)
                UIManager.PlaySFX(GetThemeValue<Resource>("buttonUpAudio"));
        }

        prevPressed = pressed;
        prevHover = hover;
        prevMouseDown = mouseDown;

        // handle colour
        material.SetShaderParameter("mainColour", bg.zeroToOne);
    }*/

    private void PlaySFX(string name)
    {
        if (GetThemeValue<Resource>(name) is not null)
            UIManager.PlaySFX(GetThemeValue<Resource>(name));
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        if (pressed)
            buttonHold.Invoke(this);

        BearingColour bg;
        if (UIManager.GetHoveredElement() == this && !pressed)
        {
            bg = GetThemeValue<BearingColour>("buttonHoverBackground");
        }
        else if (pressed)
        {
            bg = GetThemeValue<BearingColour>("buttonDownBackground");
        }
        else
        {
            bg = GetThemeValue<BearingColour>("buttonUpBackground");
        }

        material.SetShaderParameter("mainColour", bg.zeroToOne);

        if (UIManager.GetHoveredElement() == this && !prevHovered)
        {
            PlaySFX("buttonHoverAudio");
        }

        prevHovered = UIManager.GetHoveredElement() == this;
    }

    protected override void OnHandleMouseEvent()
    {
        base.OnHandleMouseEvent();

        if (Input.GetMouseButtonUp(0) && pressed)
        {
            buttonReleased.Invoke(this);
            PlaySFX("buttonUpAudio");
            pressed = false;
        }

        if (!active)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            buttonPressed.Invoke(this);
            PlaySFX("buttonDownAudio");
            pressed = true;
        }
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
    public event Action onMoved = ()=>{};
    private int _handleHeight = 10;
    public int handleHeight
    {
        get
        {
            return _handleHeight;
        }
        set
        {
            _handleHeight = value;

            if (button != null)
                button.size = new UDim2(1f,0,10,handleHeight);
        }
    }
    public float value = 0.5f;

    public override void OnLoad()
    {
        base.OnLoad();

        themeOverride.SetColour("panelBG", UIManager.currentTheme.GetColour("sliderBackground"));

        button = new UIButton();
        button.parent = rid;
        button.anchor = new Vector2(0.5f,0.5f);
        button.position = new UDim2(0.5f,0.5f);
        button.size = new UDim2(1f,0,10,handleHeight);
        button.renderLayer = renderLayer + 2;
        button.buttonPressed += ButtonClicked;
        gameObject.AddComponent(button);

        fill = new UIPanel();
        fill.parent = rid;
        fill.anchor = new Vector2(0.5f, 1.0f);
        fill.position = new UDim2(0.5f, 1.0f);
        fill.size = new UDim2(1f, 0.5f);
        fill.renderLayer = renderLayer + 1;
        fill.themeOverride.SetColour("panelBG", UIManager.currentTheme.GetColour("sliderFill"));
        gameObject.AddComponent(fill);
    }

    public bool IsMoving()
    {
        return dragging;
    }

    public void ButtonClicked(UIButton sender)
    {
        dragging = true;
    }

    private bool prevDragging;
    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        fill.visible = showFillLine;
        fill.size = new UDim2(1f, value);
        fill.themeOverride.SetColour("panelBG", theme.GetColour("sliderFill"));
        
        themeOverride.SetColour("panelBG", theme.GetColour("sliderBackground"));

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        button.position = new UDim2(0.5f, 1f-value);

        if (!dragging && prevDragging)
        {
            onMoved.Invoke();
        }

        if (!dragging)
        {
            prevDragging = false;
            return;
        }

        // TODO:
        // this maths doesnt work when the size offset is changed, so dont use size offset on this object until i figure out how to neatly normalise UDims with propagation
        var mouseRatio = (Input.GetMousePosition().Y-position.offset.Y) / (Game.instance.ClientSize.Y);
        var sliderTopRatio = position.scale.Y- (anchor.Y * size.scale.Y);
        var sliderBottomRatio = sliderTopRatio + size.scale.Y;
        var percent = (mouseRatio - sliderTopRatio) / (sliderBottomRatio - sliderTopRatio);
        percent = Math.Clamp(percent, 0, 1);
        value = 1f-percent;
        button.position = new UDim2(0.5f, percent);
        
        prevDragging = true;
    }

    public override void Cleanup()
    {
        base.Cleanup();

        button.Cleanup();
        fill.Cleanup();
    }
}

// THIS IS FULLY HACKED TOGETHER, I NEED TO REMAKE NOT JUST THIS WHOLE COMPONENT BUT PROBABLY MOST OF THE UI CODE AT THIS POINT
/*
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
            shader = new Shader("eng/defaultUI.vert", "eng/defaultUI.frag"),
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

        material.SetShaderParameter("mainColour", GetThemeValue<BearingColour>("verticalScrollBG").zeroToOne);
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
}*/


public class UIVerticalScrollView : UIElement
{
    public int scrollSensitivity { get; set; } = 1;
    public int spacing { get; set; } = 5;

    private List<UIElement> contents { get; set; } = new List<UIElement>();

    private int scrollAmount;

    public UIVerticalScrollView(params object[] meta) : base(meta)
    {
        material = new Material()
        {
            shader = new Shader("eng/defaultUI.vert", "eng/defaultUI.frag"),
        };
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter("mainColour", GetThemeValue<BearingColour>("verticalScrollBG").zeroToOne);

        foreach (UIElement element in contents)
        {
            if (!visible)
            {
                element.visible = false;
                element.SetActive(false, changeSetActive: false);
            }
        }
    }

    protected override void BeforeRender()
    {
        base.BeforeRender();

        for (int i = 0; i < contents.Count; i++)
        {
            UIElement element = contents[i];
            element.position = new UDim2(0,0,element.position.offset.X,(element.size.offset.Y + spacing) * (i - scrollAmount));

            if (element.position.offset.Y < 0 || element.position.offset.Y + element.worldSize.offset.Y > worldSize.Normalize(Game.instance.ClientSize).Y * Game.instance.ClientSize.Y)
            {
                element.visible = false;
                element.SetActive(false, changeSetActive: false);
            }
            else
            {
                element.visible = true;
                element.SetActive(element.GetSetActive(), changeSetActive: false);
            }
        }
    }

    protected override void OnHandleMouseEvent()
    {
        base.OnHandleMouseEvent();

        if (!active)
            return;

        scrollAmount -= (int)Input.GetMouseScrollDelta().Y;

        if (scrollAmount < 0)
            scrollAmount = 0;
    }

    public void ClearContents()
    {
        foreach (UIElement elem in contents.ToList())
        {
            if (elem != null)
            {
                elem.gameObject.RemoveComponent(elem);
            }
        }

        contents.Clear();
    }

    public List<UIElement> GetContents()
    {
        return contents;
    }

    public void RemoveElement(UIElement element)
    {
        contents.Remove(element);
    }

    public void AddElement(UIElement element)
    {
        contents.Add(element);
        element.parent = rid;
        element.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
        element.useParentActivity = false;
        element.useParentVisibility = false;
    }

    public override void Cleanup()
    {
        ClearContents();

        base.Cleanup();
    }

    public void SetScrollAmount(int amount)
    {
        scrollAmount = amount;
    }

    public int GetScrollAmount()
    {
        return scrollAmount;
    }
}