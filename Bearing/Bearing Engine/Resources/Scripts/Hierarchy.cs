﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;

public class Hierarchy : Component
{
    public static Hierarchy instance;

    private UIVerticalScrollView scrollView;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        instance = this;

        scrollView = (UIVerticalScrollView)UIManager.FindFromID(1);

        selectionTheme = new UITheme();
        selectionTheme.buttonUpBackground = BearingColour.LightBlue;
        selectionTheme.buttonDownBackground = BearingColour.LightBlue;
        selectionTheme.buttonHoverBackground = BearingColour.LightBlue;
    }

    bool tes;
    public override void OnTick(float dt)
    {
        if (!tes)
        {
            tes = true;
            UpdateView();
        }
    }

    private List<GameObject> TraverseForChildren(GameObject root)
    {
        List<GameObject> result = new List<GameObject>();

        if (root.immediateChildren.Count == 0)
        {
            result.Add(root);
            return result;
        }

        for (int i = 0; i < root.immediateChildren.Count; i++)
            result.AddRange(TraverseForChildren(root.immediateChildren[i]));

        result.Add(root);
        return result;
    }

    public void UpdateView()
    {
        // remove all UI from hierarchy
        foreach (int elem in scrollView.contents.ToList())
        {
            UIElement element = UIManager.FindFromID(elem);

            element.gameObject.RemoveComponent(element);
        }

        scrollView.contents.Clear();

        // add back new, updated UI
        List<GameObject> allObjects = TraverseForChildren(Game.instance.root).SkipLast(1).ToList();

        foreach (GameObject obj in allObjects)
        {
            if (obj.tag == "Hierarchy" || obj.tag == "Inspector")
                continue;
            
            AddHierarchyObject(obj);
        }
    }

    private void AddHierarchyObject(GameObject go)
    {
        GameObject prefab = SceneLoader.LoadFromFile("./Resources/Scene/buttonObject.json", true);
        Component newUI1 = prefab.GetComponent(0);
        Component newUI2 = prefab.GetComponent(1);
        prefab.RemoveComponent(newUI1, false);
        prefab.RemoveComponent(newUI2, false);

        gameObject.AddComponent(newUI1);
        gameObject.AddComponent(newUI2);
        newUI1.OnLoad();
        newUI2.OnLoad();

        ((UIElement)newUI1).rid = UIManager.GetUniqueUIID();

        ((UILabel)newUI2).text = go.name;
        ((UILabel)newUI2).parent = ((UIElement)newUI1).rid;

        ((UIButton)newUI1).buttonPressed += ItemSelected;
        ((UIButton)newUI1).metadata = new object[] { go.name };

        scrollView.contents.Add(((UIElement)newUI1).rid);
    }

    public int selectedID = -1;
    public string selectedName = "";
    private UITheme selectionTheme;
    public event Action itemSelected = () => { };

    private void ItemSelected(object? sender, EventArgs e)
    {
        if (selectedID != -1)
            ((UIButton)UIManager.FindFromID(selectedID)).theme = UIManager.currentTheme;

        if (selectedID == ((UIButton)sender).rid)
        {
            selectedID = -1;
            selectedName = "";
        }
        else
        {
            selectedID = ((UIButton)sender).rid;
            selectedName = (string)((UIButton)sender).metadata[0];
        }

        if (selectedID != -1)
            ((UIButton)sender).theme = selectionTheme;

        itemSelected.Invoke();
    }
}