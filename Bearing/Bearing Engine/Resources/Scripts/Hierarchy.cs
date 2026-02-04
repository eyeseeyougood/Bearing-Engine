using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
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

        scrollView = (UIVerticalScrollView)UIManager.FindFromRID(1);
        ((UIButton)UIManager.FindFromRID(7)).buttonPressed += AddGameObjectPressed;

        selectionTheme = new UITheme();
        selectionTheme.buttonUpBackground = BearingColour.LightBlue;
        selectionTheme.buttonDownBackground = BearingColour.LightBlue;
        selectionTheme.buttonHoverBackground = BearingColour.LightBlue;
    }

    private void AddGameObjectPressed(object? sender, EventArgs e)
    {
        GameObject go = new GameObject();
        go.name = "New GameObject";
        go.Load();
        go.parent = Game.instance.root;

        UpdateView();
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
            UIElement element = UIManager.FindFromRID(elem);

            if (element == null) continue;

            element.gameObject.RemoveComponent(element);
        }

        scrollView.contents.Clear();

        // add back new, updated UI
        List<GameObject> allObjects = TraverseForChildren(Game.instance.root).SkipLast(1).ToList();

        foreach (GameObject obj in allObjects)
        {
            if (obj.tag == "EditorObject" || obj.tag == "HierarchyHidden")
                continue;
            
            AddHierarchyObject(obj);
        }

        selectedID = -1;
    }

    private void AddHierarchyObject(GameObject go)
    {
        UIButton button = new UIButton();
        button.renderLayer = -1;
        button.anchor = new Vector2(0.5f, 0.5f);
        button.position = new UDim2(0.5f, 0.5f);
        button.size = new UDim2(0.0f, 0.0f, 0, 100);
        button.buttonPressed += ItemSelected;
        button.metadata = new object[] { go.id };
        gameObject.AddComponent(button);
        
        UILabel label = new UILabel();
        label.renderLayer = 0;
        label.anchor = new Vector2(0.5f, 0.5f);
        label.position = new UDim2(0.5f, 0.5f);
        label.size = new UDim2(1.0f, 1.0f);
        label.text = go.name;
        label.parent = button.rid;
        gameObject.AddComponent(label);

        scrollView.contents.Add(button.rid);
    }

    public int selectedID = -1;
    public int selectedObjID = -1;
    private UITheme selectionTheme;
    public event Action itemSelected = () => { };

    private void ItemSelected(object? sender, EventArgs e)
    {
        SelectItem((UIButton)sender);
    }

    public void SelectItem(UIButton button, bool invokeSelectedEvent = true)
    {
        if (selectedID != -1)
            ((UIButton)UIManager.FindFromRID(selectedID)).theme = UIManager.currentTheme;

        if (selectedID == button.rid)
        {
            selectedID = -1;
            selectedObjID = -1; // TODO: maybe this could cause bugs
        }
        else
        {
            selectedID = button.rid;
            selectedObjID = button.GetMeta<int>();
        }

        if (selectedID != -1)
            button.theme = selectionTheme;

        if (invokeSelectedEvent)
            itemSelected.Invoke();
    }

    public UIButton? GetHierarchyButtonForObject(GameObject go)
    {
        foreach (int elem in scrollView.contents)
        {
            UIButton element = (UIButton)UIManager.FindFromRID(elem);
            if (element.GetMeta<int>() == go.id)
            {
                return element;
            }
        }

        return null;
    }
}