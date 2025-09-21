using Bearing;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class InspectorItem : Component
{
    public GameObject linkedObject;
    public object objectComp;

    private UIVerticalScrollView scrollView;

    public override void Cleanup()
    {
        UIVerticalScrollView inspectorScroll = (UIVerticalScrollView)UIManager.FindFromRID(2);
        inspectorScroll.contents.Remove(scrollView.rid);

        scrollView.Cleanup();
    }

    public override void OnLoad()
    {
        // init scroll
        scrollView = new UIVerticalScrollView();
        scrollView.renderLayer = -1;
        scrollView.metadata = new object[] { };
        scrollView.scrollSensitivity = 0;
        scrollView.consumedInputs.Remove("Scroll");
        scrollView.theme = new UITheme() { verticalScrollBG = new BearingColour() { zeroToOne = new Vector4(0.65f, 0.65f, 0.65f, 1f) } };
        scrollView.size = new UDim2(0, 0, 0, 100);
        UIVerticalScrollView inspectorScroll = (UIVerticalScrollView)UIManager.FindFromRID(2);
        inspectorScroll.gameObject.AddComponent(scrollView);
        inspectorScroll.contents.Add(scrollView.rid);

        Type cType = objectComp.GetType();
        
        // generate all property UI
        foreach (var property in cType.GetProperties())
        {
            if (property.GetCustomAttribute<HideFromInspectorAttribute>() != null)
                continue;

            if (property.PropertyType.GetCustomAttribute<InspectorShowAttribute>() != null)
            {
                object comp = property.GetValue(objectComp);
                Inspector.instance.AddInspectorObject(linkedObject, comp);
                continue;
            }
            
            // prop title
            UIPanel propertyPanel = new UIPanel();
            propertyPanel.themeOverride.uiPanelBG = BearingColour.FromZeroToOne(new Vector3(0.87f, 0.87f, 0.87f));
            propertyPanel.renderLayer = 2;
            propertyPanel.size = new UDim2(0, 0, 0, 100);
            gameObject.AddComponent(propertyPanel);

            UILabel propertyLabel = new UILabel();
            propertyLabel.renderLayer = 5;
            propertyLabel.text = property.Name;
            propertyLabel.size = new UDim2(1, 1, 0, 0);
            propertyLabel.parent = propertyPanel.rid;
            gameObject.AddComponent(propertyLabel);
            scrollView.contents.Add(propertyPanel.rid);
            
            // value

            // panel
            UIPanel panel = new UIPanel();
            panel.size = new UDim2(0, 0, 0, 100);
            panel.renderLayer = 2;
            gameObject.AddComponent(panel);

            // field
            UIElement field = null;
            switch (property.PropertyType.Name)
            {
                default:
                    field = CreateStringField(property);
                    break;
                case "Vector3":
                    field = CreateVector3Field(property);
                    break;
                case "Boolean":
                    field = CreateBoolField(property);
                    break;
            }

            field.parent = panel.rid;

            // json edit button
            UIButton editButton = CreateJsonEditButton(property);

            editButton.parent = panel.rid;

            scrollView.contents.Add(panel.rid);
        }
        scrollView.size = new UDim2(0,0,0,100* scrollView.contents.Count + scrollView.spacing*(scrollView.contents.Count-1));
    }

    private UIButton CreateJsonEditButton(PropertyInfo property)
    {
        UIButton editButton = new UIButton();
        editButton.renderLayer = 3;
        editButton.position = new UDim2(0.85f, 0, 0, 0);
        editButton.size = new UDim2(0.15f, 1f, 0, 0);
        editButton.buttonPressed += OpenJsonEditor;
        editButton.metadata = new object[] { this, property.Name };
        gameObject.AddComponent(editButton);

        UIImage editImage = new UIImage();
        editImage.renderLayer = 6;
        editImage.size = new UDim2(1, 1, 0, 0);
        editImage.parent = editButton.rid;
        gameObject.AddComponent(editImage);
        editImage.SetTexture(Texture.LoadFromFile("./Resources/Textures/Pencil.png"));

        return editButton;
    }

    private void OpenJsonEditor(object? sender, EventArgs e)
    {
        UIButton s = (UIButton)sender;

        Inspector insp = Inspector.instance;

        Type cType = objectComp.GetType();
        PropertyInfo p = cType.GetProperty((string)s.metadata[1]);
        Type pType = p.PropertyType;

        string seriValue = Inspector.instance.SerialiseObject(p.GetValue(objectComp), pType);

        insp.SetJsonEditorText(seriValue);

        insp.LinkJsonEditor((InspectorItem)s.metadata[0], (string)s.metadata[1]);
        insp.ShowJsonEditor();
    }

    public void SetPropValue(object? sender, string newValue)
    {
        string propName = (string)((UITextBox)sender).metadata[0];

        Type cType = objectComp.GetType();
        PropertyInfo p = cType.GetProperty(propName);
        Type pType = p.PropertyType;
        string pName = p.Name;

        object newVal = JsonConvert.DeserializeObject(newValue, pType, new ColliderConverter());

        object capturedComp = objectComp;
        var currVal = p.GetValue(objectComp);

        SetNewValue(p, objectComp, newVal);

        Inspector insp = Inspector.instance;
        insp.HideJsonEditor();
        insp.UpdateView();
    }
    
    private UIElement CreateStringField(PropertyInfo property)
    {
        // textbox
        UITextBox propertyTextbox = new UITextBox();
        propertyTextbox.renderLayer = 5;
        propertyTextbox.size = new UDim2(0.85f, 1f, 0, 0);
        propertyTextbox.multiline = false;
        propertyTextbox.text = property.GetValue(objectComp).ToString();
        propertyTextbox.metadata = new object[] { property.Name, "System.String" };
        propertyTextbox.onTextSubmit += PropertyValueTextSubmit;
        gameObject.AddComponent(propertyTextbox);

        return propertyTextbox;
    }

    private UIElement CreateBoolField(PropertyInfo property)
    {
        // textbox
        UITextBox propertyTextbox = new UITextBox();
        propertyTextbox.renderLayer = 5;
        propertyTextbox.size = new UDim2(0.85f, 1f, 0, 0);
        propertyTextbox.multiline = false;
        bool testType = true;
        propertyTextbox.text = property.GetValue(objectComp).ToString();
        propertyTextbox.metadata = new object[] { property.Name, "System.Boolean" };
        propertyTextbox.onTextSubmit += PropertyValueTextSubmit;
        gameObject.AddComponent(propertyTextbox);

        return propertyTextbox;
    }

    private UIElement CreateVector3Field(PropertyInfo property)
    {
        Vector3 value = (Vector3)property.GetValue(objectComp);

        // vector panel
        UIPanel panel = new UIPanel();
        panel.size = new UDim2(0.85f, 1f, 0, 0);
        panel.renderLayer = 2;
        gameObject.AddComponent(panel);

        // v1
        UITextBox v1Textbox = new UITextBox();
        v1Textbox.renderLayer = 5;
        v1Textbox.position = new UDim2(0,0,0,0);
        v1Textbox.size = new UDim2(0.3f, 1f, 0, 0);
        v1Textbox.multiline = false;
        v1Textbox.text = value.X.ToString();
        v1Textbox.metadata = new object[] { property.Name, "X" };
        v1Textbox.onTextSubmit += PropertyValueVector3Submit;
        gameObject.AddComponent(v1Textbox);

        // v2
        UITextBox v2Textbox = new UITextBox();
        v2Textbox.renderLayer = 5;
        v2Textbox.position = new UDim2(0.35f, 0, 0, 0);
        v2Textbox.size = new UDim2(0.3f, 1f, 0, 0);
        v2Textbox.multiline = false;
        v2Textbox.text = value.Y.ToString();
        v2Textbox.metadata = new object[] { property.Name, "Y" };
        v2Textbox.onTextSubmit += PropertyValueVector3Submit;
        gameObject.AddComponent(v2Textbox);

        // v3
        UITextBox v3Textbox = new UITextBox();
        v3Textbox.renderLayer = 5;
        v3Textbox.position = new UDim2(0.7f, 0, 0, 0);
        v3Textbox.size = new UDim2(0.3f, 1f, 0, 0);
        v3Textbox.multiline = false;
        v3Textbox.text = value.Z.ToString();
        v3Textbox.metadata = new object[] { property.Name, "Z" };
        v3Textbox.onTextSubmit += PropertyValueVector3Submit;
        gameObject.AddComponent(v3Textbox);

        v1Textbox.parent = panel.rid;
        v2Textbox.parent = panel.rid;
        v3Textbox.parent = panel.rid;

        return panel;
    }

    private void PropertyValueTextSubmit(object? sender, string e)
    {
        UITextBox box = ((UITextBox)sender);
        string propName = (string)box.metadata[0];
        string type = (string)box.metadata[1];

        Type t = Type.GetType(type);
        object nVal = Convert.ChangeType(e, t);

        SetNewValue(objectComp.GetType().GetProperty(propName), objectComp, nVal);
    }

    private void PropertyValueVector3Submit(object? sender, string e)
    {
        UITextBox box = ((UITextBox)sender);
        string propName = (string)box.metadata[0];

        string part = (string)box.metadata[1];

        float f = float.Parse(e.TrimStart());

        Vector3 cVal = (Vector3)objectComp.GetType().GetProperty(propName).GetValue(objectComp);

        Vector3 nVal = part switch
        {
            "X" => new Vector3(f, cVal.Y, cVal.Z),
            "Y" => new Vector3(cVal.X, f, cVal.Z),
            "Z" => new Vector3(cVal.X, cVal.Y, f)
        };

        SetNewValue(objectComp.GetType().GetProperty(propName), objectComp, nVal);
    }

    private void SetNewValue(PropertyInfo p, object obj, object newVal)
    {
        object capturedComp = obj;
        var currVal = p.GetValue(obj);

        CommandManager.Do(() =>
        {
            p.SetValue(capturedComp, newVal);
            Inspector.instance.UpdateView();
        }, () =>
        {
            p.SetValue(capturedComp, currVal);
            Inspector.instance.UpdateView();
        });
    }

    public override void OnTick(float dt)
    {
    }
}