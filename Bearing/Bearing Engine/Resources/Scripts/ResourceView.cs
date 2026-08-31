using Bearing;

public class ResourceView : Component
{
    public static ResourceView? instance;
    
    public CustomTextBox? resourcesPath;

    private UIVerticalScrollView scroll;

    private int panel;

    public ResourceView(UIVerticalScrollView scroll) { instance = this; this.scroll = scroll; panel = scroll.parent; }

    public override void OnLoad()
    {
        resourcesPath = new CustomTextBox();
        resourcesPath.theme = UIManager.themes["Resources"];
        resourcesPath.renderLayer = 1;
        resourcesPath.parent = panel;
        resourcesPath.position = new UDim2(0.2f, 0.15f);
        resourcesPath.size = new UDim2(0.6f, 0f, -55, 50);
        resourcesPath.placeholderText = "Enter path to resources here..";
        if (Path.Exists("./EditorData/resourcesPath.txt"))
            resourcesPath.text = File.ReadAllText("./EditorData/resourcesPath.txt");
        resourcesPath.textSubmitted += (t) => {
            if (!Directory.Exists("./EditorData/"))
                Directory.CreateDirectory("./EditorData/");
            File.WriteAllText("./EditorData/resourcesPath.txt", t.text);
        };
        gameObject.AddComponent(resourcesPath);

        CustomButton refreshButton = new CustomButton();
        refreshButton.theme = UIManager.themes["Resources"];
        refreshButton.renderLayer = 1;
        refreshButton.parent = panel;
        refreshButton.position = new UDim2(0.8f, 0.15f, -50, 0);
        refreshButton.size = new UDim2(0f, 0f, 50, 50);
        refreshButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(200,200,200,50));
        refreshButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(150,150,150,50));
        refreshButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(255,255,255,50));
        refreshButton.buttonPressed += (b) => {
            UpdateView();
        };
        gameObject.AddComponent(refreshButton);

        UIImage refreshIcon = new UIImage();
        refreshIcon.renderLayer = 0;
        refreshIcon.parent = refreshButton.rid;
        refreshIcon.position = new UDim2(0f, 0f, 5, 5);
        refreshIcon.size = new UDim2(1f, 1f, -10, -10);
        refreshIcon.SetTexture(Texture.LoadFromResource(EmbeddedResource.GetTexture("RefreshIcon.png")));
        gameObject.AddComponent(refreshIcon);

        UpdateView();
    }

    public override void OnTick(float dt) {}
    public override void Cleanup() {}

    public void UpdateView()
    {
        scroll.ClearContents();

        AddFolder(resourcesPath?.text == null ? "" : resourcesPath.text, 0, 0);

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
        item.size = new UDim2(1f, 0f, indent * -indentSize, 60);
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