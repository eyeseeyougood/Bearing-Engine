using Bearing;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Inspector : Component
{
    public static Inspector instance;

    private UIVerticalScrollView scrollView;
    private UITextBox addCompTextBox;
    private UIVerticalScrollView jsonEditor;
    private UITextBox jsonEditorTextBox;

    private StringWriter sw = new StringWriter();
    private JsonTextWriter writer;
    private JsonSerializer serialiser;
    public override void Cleanup()
    {
        sw.Close();
        writer.Close();
    }

    public string SerialiseObject(object obj, Type t)
    {
        string seriValue = "Failed to parse object!";

        sw.GetStringBuilder().Clear();
        serialiser.Serialize(writer, obj, t);
        seriValue = sw.ToString();

        return seriValue;
    }

    public override void OnLoad()
    {
        instance = this;

        writer = new JsonTextWriter(sw);
        writer.Formatting = Formatting.Indented;
        writer.IndentChar = ' ';
        writer.Indentation = 4;

        serialiser = JsonSerializer.Create(new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new ShaderParamConverter(),
                new Vector4Converter(),
                new Vector3Converter(),
                new Vector2Converter(),
                new ColliderConverter()
            }
        });

        scrollView = (UIVerticalScrollView)UIManager.FindFromRID(2);

        Hierarchy.instance.itemSelected += UpdateView;
        ((UIButton)UIManager.FindFromRID(4)).buttonPressed += AddCompButtonPressed;

        addCompTextBox = new UITextBox();
        addCompTextBox.anchor = new Vector2(0.5f,0.5f);
        addCompTextBox.position = new UDim2(0.5f,0.5f);
        addCompTextBox.size = new UDim2(0,0,200f,100f);
        addCompTextBox.visible = false;
        addCompTextBox.onTextSubmit += AddComponentToObject;
        gameObject.AddComponent(addCompTextBox);

        InitJsonEditor();

        PluginManager.InitManager();
        Game.instance.rootLoaded += PluginManager.InitPlugins;
    }

    public void SetJsonEditorText(string newValue)
    {
        jsonEditorTextBox.text = newValue.Replace("\r","");
        jsonEditorTextBox.CaretToEnd();
        jsonEditorTextBox.size = new UDim2(1,1,0,50*jsonEditorTextBox.text.Split("\n").Length);
    }

    public void ShowJsonEditor()
    {
        jsonEditor.visible = true;
    }

    public void HideJsonEditor()
    {
        jsonEditor.visible = false;
        UIManager.SendEvent(jsonEditor, "MouseExit");
    }

    public void LinkJsonEditor(InspectorItem iI, string propertyName)
    {
        jsonEditorTextBox.metadata = new object[] { propertyName };
        jsonEditorTextBox.ClearSubmitEventSubscribers();
        jsonEditorTextBox.onTextSubmit += iI.SetPropValue;
    }

    private void InitJsonEditor()
    {
        UIVerticalScrollView panel = new UIVerticalScrollView();
        panel.renderLayer = 10;
        panel.anchor = new Vector2(0.5f, 0.0f);
        panel.position = new UDim2(0.5f, 0.0f, 0, 0);
        panel.size = new UDim2(0.5f, 1f, 0, 0);
        panel.visible = false;
        panel.clipContents = false;
        panel.scrollByComponents = false;
        panel.scrollSensitivity = 10;
        gameObject.AddComponent(panel);

        UITextBox textBox = new UITextBox();
        textBox.renderLayer = 12;
        textBox.anchor = new Vector2(0.5f, 0.5f);
        textBox.position = new UDim2(0.5f, 0.5f, 0, 0);
        textBox.size = new UDim2(1f, 1f, 0, 0);
        textBox.font = "Consolas";
        gameObject.AddComponent(textBox);

        panel.contents.Add(textBox.rid);

        jsonEditor = panel;
        jsonEditorTextBox = textBox;
    }

    private void AddComponentToObject(object? sender, string e)
    {
        GameObject selected = GameObject.Find(Hierarchy.instance.selectedObjID);

        Type nType = Type.GetType(e);
        if (nType == null)
        {
            nType = Type.GetType("Bearing."+e);
        }
        if (nType == null) { CloseAddCompMenu(); return; }
        Component newComp = (Component)Activator.CreateInstance(nType);
        selected.AddComponent(newComp);

        // close menu
        CloseAddCompMenu();

        // refresh inspector
        UpdateView();
    }

    private void CloseAddCompMenu()
    {
        addCompTextBox.visible = false;
        addCompTextBox.SetTextWithoutEventTrigger(" ");
    }

    private void AddCompButtonPressed(object? sender, EventArgs e)
    {
        addCompTextBox.visible = true;
    }

    public override void OnTick(float dt)
    {
        CommandManager.Tick();
    }

    public void UpdateView()
    {
        // remove all UI from inspector
        foreach (Component c in gameObject.components.ToList())
        {
            if (c.GetType() != typeof(InspectorItem))
                continue;

            gameObject.RemoveComponent(c);
        }

        scrollView.ClearContents();
        // add back new, updated UI
        if (Hierarchy.instance.selectedID == -1)
            return;

        GameObject selected = GameObject.Find(Hierarchy.instance.selectedObjID);

        AddInspectorObject(selected, selected.transform);
        foreach (Component c in selected.components.ToList())
        {
            AddInspectorObject(selected, c);
        }
    }

    public void AddInspectorObject(GameObject linkedObj, object objectComp)
    {
        UIButton button = new UIButton();
        button.renderLayer = -2;
        button.anchor = new Vector2(0.0f, 1.0f);
        button.position = new UDim2(0.4f, 1.0f);
        button.size = new UDim2(0.2f, 0, 0, 100);
        gameObject.AddComponent(button);

        UILabel label = new UILabel();
        label.renderLayer = 0;
        label.anchor = new Vector2(0.5f, 0.5f);
        label.position = new UDim2(0.5f, 0.5f);
        label.size = new UDim2(1f, 1f, -20, -20);
        label.text = objectComp.GetType().Name;
        label.parent = button.rid;
        gameObject.AddComponent(label);

        scrollView.contents.Add(button.rid);

        InspectorItem iI = new InspectorItem();
        iI.linkedObject = linkedObj;
        iI.objectComp = objectComp;
        gameObject.AddComponent(iI);
    }
}