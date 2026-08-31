using Bearing;

public class MiniResourceView : Component
{
    public static MiniResourceView? instance;
    
    private UIVerticalScrollView scroll;
    
    private CustomButton? refreshButton;
    private UILabel? notFoundText;

    private int panel;

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

        UpdateView();
    }

    public override void OnTick(float dt) {}
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

        AddFolder(resourcesPath, 0, 0);

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
            Logger.Log(UIManager.currentTheme.ExportValues(UITheme.ThemeExportColourPrecisionMode.ZeroTo255));
            if (!ContainsPath(path + itemName))
                AddFolder(path + itemName, scroll.GetElementIndex(b) + 1, b.GetMeta<int>(2) + 1);
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