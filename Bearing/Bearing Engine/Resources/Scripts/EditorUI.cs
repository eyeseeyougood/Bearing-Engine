using OpenTK.Mathematics;
using System.Reflection;
using Bearing;

public class EditorUI : Component
{
	private CustomPanel topPanel;
    private UIPanel currentView;
    private UIPanel editorView;
    private UIPanel pluginView;

    public override void OnLoad()
    {
    	UIManager.currentTheme.SetColour("labelText", BearingColour.FromZeroTo255(230,230,230));
        UIManager.currentTheme.SetColour("panelBG", BearingColour.FromZeroTo255(19,18,19));
    	UpdateThemeHighlight(BearingColour.FromZeroTo255(209,136,227));

    	Game.instance.SetClearColour(BearingColour.FromZeroTo255(19,13,18));

        CreateEditorView();
        CreatePluginView();
    }

    private void CreateEditorView()
    {
        editorView = new UIPanel();
        editorView.size = new UDim2(1,1);
        editorView.themeOverride.SetColour("panelBG", BearingColour.Transparent);
        editorView.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(editorView);

        currentView = editorView;

        CreateHeirarchy();
        CreateTopBar();
        CreateComponentView();
        CreateBottomBar();
        CreateSceneView();
    }

    private void CreatePluginView()
    {
        pluginView = new UIPanel();
        pluginView.size = new UDim2(1,1);
        pluginView.themeOverride.SetColour("panelBG", BearingColour.Transparent);
        pluginView.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        pluginView.useParentActivity = false;
        pluginView.useParentVisibility = false;
        gameObject.AddComponent(pluginView);

        new PluginManager();
        if (PluginManager.instance is not null)
            PluginManager.instance.LoadPlugins();

        UIVerticalScrollView scroll = new UIVerticalScrollView();
        scroll.renderLayer = 2;
        scroll.parent = pluginView.rid;
        scroll.anchor = new Vector2(0.5f, 0.5f);
        scroll.position = new UDim2(0.5f, 0.5f);
        scroll.size = new UDim2(0.8f, 0.6f);
        scroll.themeOverride.SetColour("verticalScrollBG", BearingColour.Transparent);
        gameObject.AddComponent(scroll);

        if (PluginManager.instance is null)
            return;

        foreach (Plugin plugin in PluginManager.instance.loadedPlugins)
        {
            CustomPanel pluginUI = new CustomPanel();
            pluginUI.renderLayer = 3;
            pluginUI.size = new UDim2(1f, 0, 0, 100);
            pluginUI.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(145,124,227));
            gameObject.AddComponent(pluginUI);

            CustomButton toggleButton = new CustomButton();
            toggleButton.renderLayer = 4;
            toggleButton.parent = pluginUI.rid;
            toggleButton.position = new UDim2(0, 0, 10, 10);
            toggleButton.size = new UDim2(1, 1, -20, -20);
            toggleButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
            toggleButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
            toggleButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
            toggleButton.themeOverride.SetColour("panelOutline", plugin.isEnabled ? BearingColour.FromZeroTo255(0,255,107) : BearingColour.FromZeroTo255(255,0,107));
            toggleButton.buttonPressed += (s) => {
                PluginManager.instance.TogglePluginEnabled(plugin);
                s.themeOverride.SetColour("panelOutline", plugin.isEnabled ? BearingColour.FromZeroTo255(0,255,107) : BearingColour.FromZeroTo255(255,0,107));
            };
            toggleButton.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
            gameObject.AddComponent(toggleButton);

            UILabel toggleLabel = new UILabel();
            toggleLabel.renderLayer = 5;
            toggleLabel.parent = toggleButton.rid;
            toggleLabel.position = new UDim2(0.05f,0.05f,8,8);
            toggleLabel.size = new UDim2(0.9f,0.9f,-16,-16);
            toggleLabel.text = plugin.displayName;
            toggleLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
            toggleLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
            gameObject.AddComponent(toggleLabel);

            scroll.AddElement(pluginUI);
        }

        UIManager.Sort();

        pluginView.visible = false;
        pluginView.active = false;
    }

    private void UpdateThemeHighlight(BearingColour highlight)
    {
    	UIManager.currentTheme.SetColour("panelOutline", highlight);
    }

    private void CreateHeirarchy()
    {
    	Hierarchy h = new Hierarchy(editorView.rid);
    	gameObject.AddComponent(h);
    }

    private void CreateTopBar()
    {
    	topPanel = new CustomPanel();
        topPanel.renderLayer = 1;
    	topPanel.position = new UDim2(0.2f, 0f);
    	topPanel.size = new UDim2(0.6f, 0.1f);
		topPanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(145,124,227));
    	gameObject.AddComponent(topPanel);

        // Editor View
        CustomButton editorButton = new CustomButton("Editor View");
        editorButton.renderLayer = 2;
        editorButton.parent = topPanel.rid;
        editorButton.position = new UDim2(0, 0, 8, 8);
        editorButton.size = new UDim2(0.25f, 1, -8, -16);
        editorButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        editorButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        editorButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        editorButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        editorButton.buttonPressed += SwitchView;
        gameObject.AddComponent(editorButton);

        UILabel editorLabel = new UILabel();
        editorLabel.renderLayer = 3;
        editorLabel.parent = editorButton.rid;
        editorLabel.position = new UDim2(0.05f,0.05f,8,8);
        editorLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        editorLabel.text = "Editor View";
        editorLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        editorLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(editorLabel);

        // Resources View
        CustomButton resourcesView = new CustomButton();
        resourcesView.renderLayer = 2;
        resourcesView.parent = topPanel.rid;
        resourcesView.position = new UDim2(0.25f, 0, 4, 8);
        resourcesView.size = new UDim2(0.25f, 1, -4, -16);
        resourcesView.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        resourcesView.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        resourcesView.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        resourcesView.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        gameObject.AddComponent(resourcesView);

        UILabel resourcesLabel = new UILabel();
        resourcesLabel.renderLayer = 3;
        resourcesLabel.parent = resourcesView.rid;
        resourcesLabel.position = new UDim2(0.05f,0.05f,8,8);
        resourcesLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        resourcesLabel.text = "Resources";
        resourcesLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        resourcesLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(resourcesLabel);

        // Plugin View
        CustomButton pluginButton = new CustomButton("Plugin View");
        pluginButton.renderLayer = 2;
        pluginButton.parent = topPanel.rid;
        pluginButton.position = new UDim2(0.5f, 0, 4, 8);
        pluginButton.size = new UDim2(0.25f, 1, -4, -16);
        pluginButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        pluginButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        pluginButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        pluginButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        pluginButton.buttonPressed += SwitchView;
        gameObject.AddComponent(pluginButton);

        UILabel pluginLabel = new UILabel();
        pluginLabel.renderLayer = 3;
        pluginLabel.parent = pluginButton.rid;
        pluginLabel.position = new UDim2(0.05f,0.05f,8,8);
        pluginLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        pluginLabel.text = "Plugins";
        pluginLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        pluginLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(pluginLabel);

        // Shaders View
        CustomButton shadersView = new CustomButton();
        shadersView.renderLayer = 2;
        shadersView.parent = topPanel.rid;
        shadersView.position = new UDim2(0.75f, 0, 4, 8);
        shadersView.size = new UDim2(0.25f, 1, -12, -16);
        shadersView.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        shadersView.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        shadersView.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        shadersView.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        gameObject.AddComponent(shadersView);

        UILabel shadersLabel = new UILabel();
        shadersLabel.renderLayer = 3;
        shadersLabel.parent = shadersView.rid;
        shadersLabel.position = new UDim2(0.05f,0.05f,8,8);
        shadersLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        shadersLabel.text = "Shaders";
        shadersLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        shadersLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(shadersLabel);
    }

    private void SwitchView(UIButton sender)
    {
        currentView.visible = false;
        currentView.active = false;

        switch ((string)sender.metadata[0])
        {
            case "Plugin View":
                currentView = pluginView;
                break;
            default:
                currentView = editorView;
                break;
        }

        currentView.visible = true;
        currentView.active = true;
    }

    private void CreateComponentView()
    {
    	ComponentView h = new ComponentView(editorView.rid);
        gameObject.AddComponent(h);
    }

    private void CreateBottomBar()
    {
    	CustomPanel bottomPanel = new CustomPanel();
    	bottomPanel.parent = editorView.rid;
    	bottomPanel.position = new UDim2(0.2f, 0.7f);
    	bottomPanel.size = new UDim2(0.6f, 0.3f);
		bottomPanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(145,124,227));
    	gameObject.AddComponent(bottomPanel);

        CustomButton createGOButton = new CustomButton();
        createGOButton.renderLayer = 2;
        createGOButton.parent = bottomPanel.rid;
        createGOButton.position = new UDim2(0, 0, 10, 10);
        createGOButton.size = new UDim2(0.333333f, 0.5f, -15, -15);
        createGOButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        createGOButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        createGOButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        createGOButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        createGOButton.buttonPressed += (b) => {
            GameObject go = new GameObject();
            go.name = "Empty GameObject";
            go.parent = Game.instance.root;
            go.Load();

            Hierarchy.instance?.CreateObjectButton(go, 1);
            UIManager.Sort();
        };
        gameObject.AddComponent(createGOButton);

        UILabel createGOLabel = new UILabel();
        createGOLabel.renderLayer = 3;
        createGOLabel.parent = createGOButton.rid;
        createGOLabel.position = new UDim2(0.05f,0.05f,8,8);
        createGOLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        createGOLabel.text = "Create GameObject";
        createGOLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        createGOLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(createGOLabel);

        CustomButton exportSceneButton = new CustomButton();
        exportSceneButton.renderLayer = 2;
        exportSceneButton.parent = bottomPanel.rid;
        exportSceneButton.position = new UDim2(0, 0.5f, 10, 0);
        exportSceneButton.size = new UDim2(0.333333f, 0.5f, -15, -10);
        exportSceneButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        exportSceneButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        exportSceneButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        exportSceneButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        exportSceneButton.buttonPressed += (b) => {
            List<object> ignores = new List<object>()
            {
                Game.instance.root.GetComponent<AudioSource>(),
            };

            foreach (GameObject child in Game.instance.root.immediateChildren)
            {
                string? meta = child.GetMeta<string>();
                if (meta is not null)
                    if (meta == "___HIDEFROMHIERARCHY___")
                        ignores.Add(child);
            }

            String s = SceneLoader.SerialiseGameObject(Game.instance.root, ignores.ToArray());
            if (!Directory.Exists("./Export"))
                Directory.CreateDirectory("./Export");
            File.WriteAllText("./Export/main.bst", s);
        };
        gameObject.AddComponent(exportSceneButton);

        UILabel exportSceneLabel = new UILabel();
        exportSceneLabel.renderLayer = 3;
        exportSceneLabel.parent = exportSceneButton.rid;
        exportSceneLabel.position = new UDim2(0.05f,0.05f,8,8);
        exportSceneLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        exportSceneLabel.text = "Export Scene";
        exportSceneLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        exportSceneLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(exportSceneLabel);

        CustomButton importSceneButton = new CustomButton();
        importSceneButton.renderLayer = 2;
        importSceneButton.parent = bottomPanel.rid;
        importSceneButton.position = new UDim2(0.333333f, 0.5f, 0, 0);
        importSceneButton.size = new UDim2(0.333333f, 0.5f, -5, -10);
        importSceneButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        importSceneButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        importSceneButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        importSceneButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        importSceneButton.buttonPressed += (b) => {
            if (!File.Exists("./Export/main.bst"))
                return;

            foreach (GameObject child in Game.instance.root.immediateChildren.ToList())
            {
                string? meta = child.GetMeta<string>();
                if (meta is not null)
                    if (meta == "___HIDEFROMHIERARCHY___")
                        continue;
                        
                child.Cleanup();
            }

            String s = File.ReadAllText("./Export/main.bst");
            GameObject imported = SceneLoader.DeserialiseGameObject(s);
            foreach (GameObject child in imported.immediateChildren.ToList())
            {
                child.parent = Game.instance.root;
                child.Load();
            }

            imported.immediateChildren.Clear();
            imported.Cleanup();

            Hierarchy.instance?.UpdateHierarchy();
        };
        gameObject.AddComponent(importSceneButton);

        UILabel importSceneLabel = new UILabel();
        importSceneLabel.renderLayer = 3;
        importSceneLabel.parent = importSceneButton.rid;
        importSceneLabel.position = new UDim2(0.05f,0.05f,8,8);
        importSceneLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        importSceneLabel.text = "Import Scene";
        importSceneLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        importSceneLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(importSceneLabel);

        CustomButton exportPresetButton = new CustomButton();
        exportPresetButton.renderLayer = 2;
        exportPresetButton.parent = bottomPanel.rid;
        exportPresetButton.position = new UDim2(0.666666f, 0.5f, 0, 0);
        exportPresetButton.size = new UDim2(0.333333f, 0.5f, -10, -10);
        exportPresetButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        exportPresetButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        exportPresetButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        exportPresetButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        exportPresetButton.buttonPressed += (b) => {
            GameObject? exp = Hierarchy.instance?.selectedObject;

            if (exp is null)
                return;

            String s = SceneLoader.SerialiseGameObject(exp);
            if (!Directory.Exists("./Export"))
                Directory.CreateDirectory("./Export");
            File.WriteAllText($"./Export/{exp.name}.bst", s);
        };
        gameObject.AddComponent(exportPresetButton);

        UILabel exportPresetLabel = new UILabel();
        exportPresetLabel.renderLayer = 3;
        exportPresetLabel.parent = exportPresetButton.rid;
        exportPresetLabel.position = new UDim2(0.05f,0.05f,8,8);
        exportPresetLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        exportPresetLabel.text = "Export Preset";
        exportPresetLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        exportPresetLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(exportPresetLabel);

        UIVerticalScrollView importDropUpMenu = new UIVerticalScrollView();
        importDropUpMenu.renderLayer = 5;
        importDropUpMenu.parent = editorView.rid;
        importDropUpMenu.position = new UDim2(0.6f, 0.15f, 0, 0);
        importDropUpMenu.size = new UDim2(0.2f, 0.55f, -10, 0);
        importDropUpMenu.visible = false;
        importDropUpMenu.active = false;
        importDropUpMenu.useParentActivity = false;
        importDropUpMenu.useParentVisibility = false;
        gameObject.AddComponent(importDropUpMenu);

        CustomButton importPresetButton = new CustomButton();
        importPresetButton.renderLayer = 2;
        importPresetButton.parent = bottomPanel.rid;
        importPresetButton.position = new UDim2(0.666666f, 0f, 0, 10);
        importPresetButton.size = new UDim2(0.333333f, 0.5f, -10, -15);
        importPresetButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        importPresetButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        importPresetButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        importPresetButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        importPresetButton.buttonPressed += (b) => {
            importDropUpMenu.visible = !importDropUpMenu.visible;
            importDropUpMenu.active = importDropUpMenu.visible;

            if (importDropUpMenu.visible)
            {
                importDropUpMenu.ClearContents();

                if (Directory.Exists("./Export"))
                {
                    foreach (string path in Directory.GetFiles("./Export"))
                    {
                        if (path.Split("/").Last() == "main.bst")
                            continue;

                        CustomButton button = new CustomButton();
                        button.renderLayer = 6;
                        button.position = new UDim2(0,0,0,0);
                        button.size = new UDim2(1,0,0,40);
                        button.borderWidth = 4;
                        button.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(211,125,199));
                        button.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(29,28,29));
                        button.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(19,18,19));
                        button.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(9,8,9));
                        button.visible = true;
                        button.buttonPressed += (b) => {
                            string path = b.GetMeta<string>();

                            if (!File.Exists(path))
                                return;

                            String s = File.ReadAllText(path);
                            GameObject imported = SceneLoader.DeserialiseGameObject(s);
                            imported.parent = Game.instance.root;
                            imported.Load();

                            importDropUpMenu.visible = false;
                            importDropUpMenu.active = false;

                            Hierarchy.instance?.UpdateHierarchy();
                        };
                        button.AddMeta(path);
                        gameObject.AddComponent(button);

                        UILabel label = new UILabel();
                        label.parent = button.rid;
                        label.renderLayer = 7;
                        label.position = new UDim2(0,0,8,8);
                        label.size = new UDim2(1,1,-16,-16);
                        label.text = path.Split("/").Last();
                        label.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
                        label.useParentVisibility = true;
                        gameObject.AddComponent(label);

                        importDropUpMenu.AddElement(button);
                    }

                    UIManager.Sort();
                }
            }
        };
        gameObject.AddComponent(importPresetButton);

        UILabel importPresetLabel = new UILabel();
        importPresetLabel.renderLayer = 3;
        importPresetLabel.parent = importPresetButton.rid;
        importPresetLabel.position = new UDim2(0.05f,0.05f,8,8);
        importPresetLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        importPresetLabel.text = "Import Preset";
        importPresetLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        importPresetLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(importPresetLabel);


        UIVerticalScrollView addCompDropUpMenu = new UIVerticalScrollView();
        addCompDropUpMenu.renderLayer = 5;
        addCompDropUpMenu.parent = editorView.rid;
        addCompDropUpMenu.position = new UDim2(0.4f, 0.15f, 0, 0);
        addCompDropUpMenu.size = new UDim2(0.2f, 0.55f, -5, 0);
        addCompDropUpMenu.visible = false;
        addCompDropUpMenu.active = false;
        addCompDropUpMenu.useParentActivity = false;
        addCompDropUpMenu.useParentVisibility = false;
        gameObject.AddComponent(addCompDropUpMenu);

        CustomButton addCompButton = new CustomButton();
        addCompButton.renderLayer = 2;
        addCompButton.parent = bottomPanel.rid;
        addCompButton.position = new UDim2(0.333333f, 0f, 0, 10);
        addCompButton.size = new UDim2(0.333333f, 0.5f, -5, -15);
        addCompButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        addCompButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        addCompButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        addCompButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        addCompButton.buttonPressed += (b) => {
            addCompDropUpMenu.visible = !addCompDropUpMenu.visible;
            addCompDropUpMenu.active = addCompDropUpMenu.visible;

            if (addCompDropUpMenu.visible)
            {
                addCompDropUpMenu.ClearContents();

                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (!t.IsSubclassOf(typeof(Component)))
                        continue;

                    CustomButton button = new CustomButton();
                    button.renderLayer = 6;
                    button.position = new UDim2(0,0,0,0);
                    button.size = new UDim2(1,0,0,40);
                    button.borderWidth = 4;
                    button.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(211,125,199));
                    button.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(29,28,29));
                    button.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(19,18,19));
                    button.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(9,8,9));
                    button.visible = true;
                    button.buttonPressed += (b) => {
                        GameObject? go = Hierarchy.instance?.selectedObject;
                        if (go is null)
                            return;

                        try{
                            Component newComponent = (Component)Activator.CreateInstance(b.GetMeta<Type>());

                            Hierarchy.instance.selectedObject.AddComponent(newComponent);

                            ComponentView.instance?.CreateComponentPanel(newComponent);
                            UIManager.Sort();
                        }catch(Exception e) {Logger.LogError("Failed to create component: " + e.Message);}

                        addCompDropUpMenu.active = false;
                        addCompDropUpMenu.visible = false;
                    };
                    button.AddMeta(t);
                    gameObject.AddComponent(button);

                    UILabel label = new UILabel();
                    label.parent = button.rid;
                    label.renderLayer = 7;
                    label.position = new UDim2(0,0,8,8);
                    label.size = new UDim2(1,1,-16,-16);
                    label.text = t.Name;
                    label.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
                    label.useParentVisibility = true;
                    gameObject.AddComponent(label);

                    addCompDropUpMenu.AddElement(button);
                }

                UIManager.Sort();
            }
        };
        gameObject.AddComponent(addCompButton);

        UILabel addCompLabel = new UILabel();
        addCompLabel.renderLayer = 3;
        addCompLabel.parent = addCompButton.rid;
        addCompLabel.position = new UDim2(0.05f,0.05f,8,8);
        addCompLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        addCompLabel.text = "Add Component";
        addCompLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        addCompLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(addCompLabel);
    }

    private void CreateSceneView()
    {
    	CustomPanel scenePanel = new CustomPanel();
    	scenePanel.parent = editorView.rid;
    	scenePanel.position = new UDim2(0.2f, 0.1f);
    	scenePanel.size = new UDim2(0.6f, 0.6f);
		scenePanel.themeOverride.SetColour("panelBG", BearingColour.Transparent);
		scenePanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(227,112,213));
		scenePanel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;

    	gameObject.AddComponent(scenePanel);
    }

    public override void OnTick(float dt) { if (PluginManager.instance is not null) { PluginManager.instance.Tick(dt); } }
    public override void Cleanup() {}
}