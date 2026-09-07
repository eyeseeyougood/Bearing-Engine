using OpenTK.Mathematics;
using System.Reflection;
using Bearing;

public class MiniResourceView : Component
{
    public static MiniResourceView? instance;
    
    private UIVerticalScrollView scroll;
    
    private CustomButton? refreshButton;
    private UILabel? notFoundText;

    private int panel;

    public CustomButton? currentlyDragging = null;
    private CustomPanel? dragPanel;
    private UILabel? dragText;

    public MiniResourceView(UIVerticalScrollView scroll) { instance = this; this.scroll = scroll; panel = scroll.parent; }

    public override void OnLoad()
    {
        refreshButton = new CustomButton();
        refreshButton.theme = UIManager.themes["Resources"];
        refreshButton.renderLayer = 3;
        refreshButton.parent = panel;
        refreshButton.anchor = new OpenTK.Mathematics.Vector2(1f, 0.0f);
        refreshButton.position = new UDim2(1f, 0.0f, -10, 10);
        refreshButton.size = new UDim2(0.0f, 0.0f, 50, 50);
        refreshButton.borderWidth = 2;
        refreshButton.useParentActivity = false;
        refreshButton.useParentVisibility = false;
        refreshButton.buttonPressed += (b) => {
            UpdateView();
        };
        gameObject.AddComponent(refreshButton);

        UIImage refreshIcon = new UIImage();
        refreshIcon.renderLayer = 2;
        refreshIcon.parent = refreshButton.rid;
        refreshIcon.position = new UDim2(0f, 0f, 5, 5);
        refreshIcon.size = new UDim2(1f, 1f, -10, -10);
        refreshIcon.SetTexture(Texture.LoadFromResource(EmbeddedResource.GetTexture("RefreshIcon.png")));
        gameObject.AddComponent(refreshIcon);

        notFoundText = new UILabel();
        notFoundText.renderLayer = 3;
        notFoundText.parent = panel;
        notFoundText.position = new UDim2(0f,0f,10,50);
        notFoundText.size = new UDim2(1f,1f,-20,-50);
        notFoundText.text = "No files found :D";
        notFoundText.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(notFoundText);

        dragPanel = new CustomPanel();
        dragPanel.theme = UIManager.themes["Files"];
        dragPanel.renderLayer = 200;
        dragPanel.anchor = new Vector2(0.5f, 0.5f);
        dragPanel.size = new UDim2(0f,0,200,30);
        dragPanel.borderWidth = 3;
        dragPanel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(dragPanel);

        dragText = new UILabel();
        dragText.theme = UIManager.themes["Files"];
        dragText.parent = dragPanel.rid;
        dragText.renderLayer = 201;
        dragText.position = new UDim2(0,0,5,5);
        dragText.size = new UDim2(1f,1f,-10,-10);
        dragText.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(dragText);

        UpdateView();
    }

    public void AssignFileToButton(UIButton button)
    {
        object? target = button.GetMeta<object>(1);
        PropertyInfo? property = button.GetMeta<PropertyInfo>();
        if (target is null || property is null)
            return;

        UILabel? label = button.GetMeta<UILabel>(2);
        
        if (label is null)
            throw new Exception("bre, idek");

        string? currentText = currentlyDragging?.GetMeta<string>(1);
        if (currentText is null)
            throw new Exception("bre, idek 2");

        string? extension = "."+currentText.Split(".").Last();

        ExpectResourceAttribute? attribute = property.GetCustomAttribute<ExpectResourceAttribute>();
        if (attribute is not null)
        {
            if (!attribute.allowedExtensions.Contains(extension)) // TODO: display warning that incorrect type, do you want to continue
                return;
        }

        string? path = currentlyDragging?.GetMeta<string>();
        string fullpath = path + currentText;
        
        label.text = fullpath;

        property?.SetValue(target, ExternalResource.FromPath(fullpath));
        ComponentView.instance.HandleCustomInspectorSetProperty(target, property);
    }

    public override void OnTick(float dt)
    {
        if (Input.GetMouseButtonUp(0) && currentlyDragging is not null)
        {
            Vector2 m = Input.GetMousePosition();

            UIButton? button = FileAssignableRegistry.GetButtonAtPosition(m);

            if (button is not null)
            {
                AssignFileToButton(button);
            }

            currentlyDragging = null;
        }

        if (dragPanel is null || dragText is null)
            return;

        dragPanel.active = currentlyDragging is not null;
        dragPanel.visible = currentlyDragging is not null;

        if (currentlyDragging is not null)
        {
            Vector2 m = Input.GetMousePosition();
            dragPanel.position = new UDim2(0,0,m.X, m.Y);
            string? itemName = currentlyDragging.GetMeta<string>(1);
            dragText.text = itemName is null ? "ERROR" : itemName;
        }
    }

    public override void Cleanup() {}

    public void UpdateView()
    {
        scroll.ClearContents();

        TryFillViewWithItems();

        bool success = scroll.GetContents().Count > 0;

        scroll.active = success;
        scroll.visible = success;

        if (notFoundText is not null)
            notFoundText.visible = !success;

        if (refreshButton is not null)
        {
            refreshButton.active = !success;
            refreshButton.visible = !success;
        }
    }

    private void TryFillViewWithItems()
    {
        if (ResourceView.instance is null)
            return;
        string? resourcesPath = ResourceView.instance.resourcesPath?.text;
        if (resourcesPath is null)
            return;

        CreateItem(string.Join("/",resourcesPath.Split("/").SkipLast(2))+"/", "EngineData/", 0, 0, "Folders");
        CreateItem(string.Join("/",resourcesPath.Split("/").SkipLast(2))+"/", "Resources/", 1, 0, "Folders");

        UIManager.Sort();
    }

    private bool ContainsPath(string path)
    {
        bool result = false;

        foreach (UIElement elem in scroll.GetContents().ToList())
        {
            if (elem.GetMeta<string>().StartsWith(path))
            {
                scroll.RemoveElement(elem);
                result = true; // DONT RETURN EARLY, must remove all elements
            }
        }

        return result;
    }

    private void HandleDrag(CustomButton item)
    {
        currentlyDragging = item;
    }

    private const int indentSize = 10;
    private void CreateItem(string path, string itemName, int index, int indent, string theme)
    {
        CustomButton item = new CustomButton(path, itemName, indent);
        item.theme = UIManager.themes[theme];
        item.renderLayer = 2;
        item.position = new UDim2(0f, 0f, indent * indentSize, 0);
        item.size = new UDim2(1f, 0f, indent * -indentSize, 30);
        item.borderWidth = 3;
        item.buttonPressed += (b) => {
            Logger.Log("pressed");
            if (!ContainsPath(path + itemName))
            {
                Logger.Log(path + itemName);
                AddFolder(path + itemName, scroll.GetElementIndex(b) + 1, b.GetMeta<int>(2) + 1);
            }

            if (File.Exists(path + itemName))
                HandleDrag(item);
        };
        gameObject.AddComponent(item);

        UILabel itemText = new UILabel();
        itemText.renderLayer = 3;
        itemText.parent = item.rid;
        itemText.position = new UDim2(0,0,5,5);
        itemText.size = new UDim2(1f,1f,-10,-10);
        itemText.text = itemName.Replace("/","");
        itemText.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(itemText);

        scroll.InsertElement(index, item);
    }

    private void AddFolder(string path, int index, int indent)
    {
        if (!Directory.Exists(path))
            return;

        int id = 0;
        foreach (string folder in Directory.GetDirectories(path))
        {
            CreateItem(path, folder.Replace(path,"") + "/", index+id, indent, "Folders");
            id++;
        }

        foreach (string file in Directory.GetFiles(path))
        {
            CreateItem(path, file.Replace(path,""), index+id, indent, "Files");
            id++;
        }

        UIManager.Sort();
    }
}