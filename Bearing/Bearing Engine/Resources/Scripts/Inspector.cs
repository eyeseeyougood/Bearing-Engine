using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;

public class Inspector : Component
{
    private UIVerticalScrollView scrollView;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        scrollView = (UIVerticalScrollView)UIManager.FindFromID(2);

        Hierarchy.instance.itemSelected += UpdateView;
    }

    public override void OnTick(float dt)
    {
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
        // reset scroll value to ensure items are in view
        scrollView.SetScrollAmount(0);

        // remove all UI from inspector
        scrollView.ClearContents();

        foreach (Component c in gameObject.components.ToList())
        {
            if (c.GetType() != typeof(InspectorItem))
                continue;

            gameObject.RemoveComponent(c, true);
        }

        scrollView.contents.Clear();

        // add back new, updated UI
        List<GameObject> allObjects = TraverseForChildren(Game.instance.root).SkipLast(1).ToList();

        if (Hierarchy.instance.selectedID == -1)
            return;

        foreach (Component c in GameObject.Find(Hierarchy.instance.selectedName).components.ToList())
        {
            AddInspectorObject(c);
        }
    }

    private void AddInspectorObject(Component c)
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

        ((UILabel)newUI2).text = c.GetType().Name;
        ((UILabel)newUI2).parent = ((UIElement)newUI1).rid;

        scrollView.contents.Add(((UIElement)newUI1).rid);

        InspectorItem iI = new InspectorItem();
        iI.compId = c.id;
        iI.linkedObject = c.gameObject;
        gameObject.AddComponent(iI);

        prefab.Cleanup();
    }
}