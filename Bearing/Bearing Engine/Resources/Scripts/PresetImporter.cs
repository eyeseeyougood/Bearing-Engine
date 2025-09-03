using Bearing;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PresetImporter : Component
{
    private UIPanel menu;
    private UIVerticalScrollView scroll;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        // init button

        UIButton button = new UIButton();
        button.renderLayer = -2;
        button.anchor = new Vector2(0.0f,0.0f);
        button.position = new UDim2(0.6f,0.0f);
        button.size = new UDim2(0.2f, 0, 0, 100);
        button.buttonPressed += ButtonPressed;
        gameObject.AddComponent(button);

        UILabel label = new UILabel();
        label.renderLayer = 0;
        label.anchor = new Vector2(0.5f, 0.5f);
        label.position = new UDim2(0.5f, 0.5f);
        label.size = new UDim2(1f, 1f, -20, -20);
        label.text = "Import Preset";
        label.parent = button.rid;
        gameObject.AddComponent(label);

        // init menu

        menu = new UIPanel();
        menu.anchor = new Vector2(0.5f, 0.5f);
        menu.position = new UDim2(0.5f, 0.5f);
        menu.size = new UDim2(0.5f, 0.5f,0,3);
        gameObject.AddComponent(menu);

        // init scroll

        scroll = new UIVerticalScrollView();
        scroll.metadata = new object[] { "PresetScroll", "PresetScroll" };
        scroll.anchor = new Vector2(0.0f, 0.0f);
        scroll.position = new UDim2(0.0f, 0.0f);
        scroll.size = new UDim2(1, 1, 0, 0);
        scroll.parent = menu.rid;
        scroll.spacing = 1;
        scroll.scrollByComponents = true;
        scroll.scrollSensitivity = 1;
        scroll.clipContents = true;
        gameObject.AddComponent(scroll);

        HideMenu();
    }

    private void ButtonPressed(object? sender, EventArgs e)
    {
        ToggleMenu();
    }

    private void PresetPressed(object? sender, EventArgs e)
    {
        HideMenu();

        object[] meta = ((UIButton)sender).metadata;
        string file = (string)meta[0];

        GameObject nRoot = SceneLoader.LoadFromFile(file, false);

        nRoot.parent = Game.instance.root;
        nRoot.Load();

        Hierarchy.instance.UpdateView();
    }

    private void ToggleMenu()
    {
        if (menu.visible)
            HideMenu();
        else
            ShowMenu();
    }

    private void ShowMenu()
    {
        menu.visible = true;

        UpdateView();
    }

    private void HideMenu()
    {
        menu.visible = false;
    }

    public void UpdateView()
    {
        // remove old files

        scroll.ClearContents();

        // add new files

        string[] files = Resources.GetFiles("./Resources/Scene/", ".preset");

        foreach (string file in files)
        {
            UIPanel panel = new UIPanel();
            panel.anchor = new Vector2(0.0f, 0.0f);
            panel.position = new UDim2(0.0f, 0.0f);
            panel.size = new UDim2(0, 0, 0, 100);
            gameObject.AddComponent(panel);

            UIButton button = new UIButton();
            button.metadata = new object[] { file };
            button.renderLayer = 1;
            button.anchor = new Vector2(0.5f, 0.5f);
            button.position = new UDim2(0.5f, 0.5f);
            button.size = new UDim2(1f, 1f, 0, 0);
            button.buttonPressed += PresetPressed;
            button.parent = panel.rid;
            gameObject.AddComponent(button);

            UILabel label = new UILabel();
            label.renderLayer = 3;
            label.anchor = new Vector2(0.5f, 0.5f);
            label.position = new UDim2(0.5f, 0.5f);
            label.size = new UDim2(1f, 1f, -20, -20);
            label.text = file.Split('/').Last();
            label.parent = button.rid;
            gameObject.AddComponent(label);

            scroll.contents.Add(panel.rid);
        }

        UIManager.Sort();
    }

    public override void OnTick(float dt)
    {
    }
}