using Bearing;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class ExamplePlugin : Plugin
{
    private List<UIElement> ui = new List<UIElement>();

    public override void OnLoad()
    {
        base.OnLoad();

        // init some ui

        UIButton button = new UIButton();
        button.renderLayer = -2;
        button.anchor = new Vector2(0.0f, 1.0f);
        button.position = new UDim2(0.4f, 1.0f);
        button.size = new UDim2(0.2f, 0, 0, 100);
        button.buttonPressed += ButtonPressed;
        gameObject.AddComponent(button);

        UILabel label = new UILabel();
        label.renderLayer = 0;
        label.anchor = new Vector2(0.5f, 0.5f);
        label.position = new UDim2(0.5f, 0.5f);
        label.size = new UDim2(1f, 1f, -20, -20);
        label.text = "Example Button";
        label.parent = button.rid;
        gameObject.AddComponent(label);

        ui.Add(button);
    }

    private void ButtonPressed(object? sender, EventArgs e)
    {
        Logger.Log("(Example Plugin): Example Button Pressed!");
    }

    public override void OnEnable()
    {
        base.OnEnable();

        // show ui
        foreach (UIElement elem in ui)
        {
            elem.visible = true;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // hide ui
        foreach (UIElement elem in ui)
        {
            elem.visible = false;
        }
    }
}