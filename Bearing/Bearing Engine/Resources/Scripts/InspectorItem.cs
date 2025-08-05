using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Bearing;
using OpenTK.Mathematics;

public class InspectorItem : Component
{
    public GameObject linkedObject;
    public int compId;

    private Component comp;
    private object objectComp;

    private UIVerticalScrollView scrollView;

    private bool isTransform;

    private List<UIElement> panels = new List<UIElement>();

    public override void Cleanup()
    {
        foreach (var panel in panels.ToList())
        {
            if (panel != null)
                panel.Cleanup();
        }

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

        // get the linked component
        comp = linkedObject.GetComponent(compId);

        if (comp == null && compId != -3)
        {
            Logger.LogError("Invalid component or linked object!");

            return;
        }

        Type cType = typeof(Transform3D);
        objectComp = linkedObject.transform;
        if (compId != -3)
        {
            cType = comp.GetType();
            objectComp = comp;
        }

        // generate all property UI
        foreach (var property in cType.GetProperties())
        {
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
            }

            field.parent = panel.rid;

            // json edit button
            UIButton editButton = CreateJsonEditButton();

            editButton.parent = panel.rid;

            panels.Add(panel);
            scrollView.contents.Add(panel.rid);
        }
        scrollView.size = new UDim2(0,0,0,100* scrollView.contents.Count + scrollView.spacing*(scrollView.contents.Count-1));
    }

    private UIButton CreateJsonEditButton()
    {
        UIButton editButton = new UIButton();
        editButton.renderLayer = 3;
        editButton.position = new UDim2(0.85f, 0, 0, 0);
        editButton.size = new UDim2(0.15f, 1f, 0, 0);
        gameObject.AddComponent(editButton);

        UIImage editImage = new UIImage();
        editImage.renderLayer = 6;
        editImage.size = new UDim2(1, 1, 0, 0);
        editImage.parent = editButton.rid;
        gameObject.AddComponent(editImage);
        editImage.SetTexture(Texture.LoadFromFile("./Resources/Textures/Pencil.png"));

        return editButton;
    }

    private UIElement CreateStringField(PropertyInfo property)
    {
        // textbox
        UITextBox propertyTextbox = new UITextBox();
        propertyTextbox.renderLayer = 5;
        propertyTextbox.size = new UDim2(0.85f, 1f, 0, 0);
        propertyTextbox.multiline = false;
        propertyTextbox.text = property.GetValue(objectComp).ToString();
        propertyTextbox.metadata = new object[] { property.Name };
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

        objectComp.GetType().GetProperty(propName).SetValue(objectComp, e);
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

        objectComp.GetType().GetProperty(propName).SetValue(objectComp, nVal);
    }

    public override void OnTick(float dt)
    {
    }
}