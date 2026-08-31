using OpenTK.Mathematics;
using System.Reflection;
using Bearing;

public class EditorUI : Component
{
	private CustomPanel topPanel;
    private UIPanel currentView;
    private UIPanel editorView;
    private UIPanel resourceView;
    private UIPanel pluginView;
    private UIPanel shaderView;

    public override void OnLoad()
    {
        UIManager.currentTheme.ImportValues(Resources.ReadAllText(EmbeddedResource.FromPath("./Resources/Themes/Editor.theme")));

        UIManager.LoadTheme("Resources", EmbeddedResource.FromPath("./Resources/Themes/Resources.theme"));
        UIManager.LoadTheme("BigButtons", EmbeddedResource.FromPath("./Resources/Themes/BigButtons.theme"));
        UIManager.LoadTheme("BigPanels", EmbeddedResource.FromPath("./Resources/Themes/BigPanels.theme"));
        UIManager.LoadTheme("ListItems", EmbeddedResource.FromPath("./Resources/Themes/ListItems.theme"));
        UIManager.LoadTheme("Objects", EmbeddedResource.FromPath("./Resources/Themes/Objects.theme"));
        UIManager.LoadTheme("Folders", EmbeddedResource.FromPath("./Resources/Themes/Folders.theme"));
        UIManager.LoadTheme("Files", EmbeddedResource.FromPath("./Resources/Themes/Files.theme"));

    	Game.instance.SetClearColour(BearingColour.FromZeroTo255(19,13,18));
        Game.instance.SetTitle("Bearing Editor");

        CreateEditorView();
        CreateResourceView();
        CreatePluginView();
        CreateShaderView();
    }

    private void CreateEditorView()
    {
        editorView = new UIPanel("Editor View");
        editorView.size = new UDim2(1,1);
        editorView.themeOverride.SetColour("panelBG", BearingColour.Transparent);
        editorView.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        editorView.useParentActivity = false;
        editorView.useParentVisibility = false;
        gameObject.AddComponent(editorView);

        currentView = editorView;

        CreateHeirarchy();
        CreateMiniResources();
        CreateTopBar();
        CreateComponentView();
        CreateBottomBar();
        CreateSceneView();
    }

    private void CreateResourceView()
    {
        resourceView = new UIPanel("Resource View");
        resourceView.size = new UDim2(1,1);
        resourceView.themeOverride.SetColour("panelBG", BearingColour.Transparent);
        resourceView.mouseCaptureMode = UIMouseCaptureMode.Consume;
        resourceView.useParentActivity = false;
        resourceView.useParentVisibility = false;
        gameObject.AddComponent(resourceView);


        CustomPanel bg = new CustomPanel();
        bg.theme = UIManager.themes["Resources"];
        bg.renderLayer = -1;
        bg.parent = resourceView.rid;
        bg.size = UDim2.One;
        gameObject.AddComponent(bg);

        CustomPanel outline = new CustomPanel();
        outline.theme = UIManager.themes["Resources"];
        outline.renderLayer = 0;
        outline.parent = resourceView.rid;
        outline.position = topPanel.position - new UDim2(0,0,5,5);;
        outline.anchor = topPanel.anchor;
        outline.size = topPanel.size + new UDim2(0,0,10,10);
        gameObject.AddComponent(outline);

        CustomPanel scrollBG = new CustomPanel();
        scrollBG.theme = UIManager.themes["Resources"];
        scrollBG.renderLayer = 0;
        scrollBG.parent = resourceView.rid;
        scrollBG.position = new UDim2(0.2f, 0.3f);
        scrollBG.size = new UDim2(0.6f, 0.6f);
        gameObject.AddComponent(scrollBG);

        UIVerticalScrollView resourceTree = new UIVerticalScrollView();
        resourceTree.renderLayer = 1;
        resourceTree.parent = bg.rid;
        resourceTree.position = new UDim2(0.2f, 0.3f, 10, 10);
        resourceTree.size = new UDim2(0.6f, 0.6f, -20, -20);
        resourceTree.themeOverride.SetColour("verticalScrollBG", BearingColour.Transparent);
        gameObject.AddComponent(resourceTree);


        gameObject.AddComponent(new ResourceView(resourceTree));


        UIManager.Sort();


        resourceView.visible = false;
        resourceView.active = false;

        MiniResourceView.instance?.UpdateView();
    }

    private void CreateShaderView()
    {
        shaderView = new UIPanel("Shader View");
        shaderView.size = new UDim2(1,1);
        shaderView.themeOverride.SetColour("panelBG", BearingColour.Transparent);
        shaderView.mouseCaptureMode = UIMouseCaptureMode.Consume;
        shaderView.useParentActivity = false;
        shaderView.useParentVisibility = false;
        gameObject.AddComponent(shaderView);


        CustomPanel bg = new CustomPanel();
        bg.theme = UIManager.themes["Resources"];
        bg.renderLayer = -1;
        bg.parent = shaderView.rid;
        bg.size = UDim2.One;
        gameObject.AddComponent(bg);

        CustomPanel outline = new CustomPanel();
        outline.theme = UIManager.themes["Resources"];
        outline.renderLayer = 0;
        outline.parent = shaderView.rid;
        outline.position = topPanel.position - new UDim2(0,0,5,5);;
        outline.anchor = topPanel.anchor;
        outline.size = topPanel.size + new UDim2(0,0,10,10);
        gameObject.AddComponent(outline);

        CustomPanel scrollBG = new CustomPanel();
        scrollBG.theme = UIManager.themes["Resources"];
        scrollBG.renderLayer = 0;
        scrollBG.parent = shaderView.rid;
        scrollBG.position = new UDim2(0.2f, 0.3f);
        scrollBG.size = new UDim2(0.6f, 0.6f);
        gameObject.AddComponent(scrollBG);

        UIManager.Sort();

        gameObject.AddComponent(new ShaderView(shaderView.rid));

        shaderView.visible = false;
        shaderView.active = false;
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
            toggleButton.themeOverride.SetColour("panelOutline", plugin.isEnabled ? BearingColour.FromZeroTo255(0,255,107) : BearingColour.FromZeroTo255(255,0,107));
            toggleButton.buttonPressed += (s) => {
                PluginManager.instance.TogglePluginEnabled(plugin);
                s.themeOverride.SetColour("panelOutline", plugin.isEnabled ? BearingColour.FromZeroTo255(0,255,107) : BearingColour.FromZeroTo255(255,0,107));
            };
            toggleButton.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
            gameObject.AddComponent(toggleButton);

            UILabel toggleLabel = new UILabel();
            toggleLabel.theme = UIManager.themes["ListItems"];
            toggleLabel.renderLayer = 5;
            toggleLabel.parent = toggleButton.rid;
            toggleLabel.position = new UDim2(0.05f,0.05f,8,8);
            toggleLabel.size = new UDim2(0.9f,0.9f,-16,-16);
            toggleLabel.text = plugin.displayName;
            toggleLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
            gameObject.AddComponent(toggleLabel);

            scroll.AddElement(pluginUI);
        }

        UIManager.Sort();

        pluginView.visible = false;
        pluginView.active = false;
    }

    private void CreateHeirarchy()
    {
    	Hierarchy h = new Hierarchy(editorView.rid);
    	gameObject.AddComponent(h);
    }

    private void CreateMiniResources()
    {
        CustomPanel scrollBG = new CustomPanel();
        scrollBG.theme = UIManager.themes["Resources"];
        scrollBG.renderLayer = 1;
        scrollBG.parent = editorView.rid;
        scrollBG.position = new UDim2(0.0f, 0.7f);
        scrollBG.size = new UDim2(0.2f, 0.3f);
        gameObject.AddComponent(scrollBG);

        UIVerticalScrollView resourceTree = new UIVerticalScrollView();
        resourceTree.renderLayer = 2;
        resourceTree.parent = scrollBG.rid;
        resourceTree.position = new UDim2(0.0f, 0.0f, 5, 5);
        resourceTree.size = new UDim2(1f, 1f, -10, -10);
        resourceTree.spacing = 0;
        gameObject.AddComponent(resourceTree);


        gameObject.AddComponent(new MiniResourceView(resourceTree));


        UIManager.Sort();
    }

    private void CreateTopBar()
    {
    	topPanel = new CustomPanel();
        topPanel.theme = UIManager.themes["BigPanels"];
        topPanel.renderLayer = 1;
    	topPanel.position = new UDim2(0.2f, 0f);
    	topPanel.size = new UDim2(0.6f, 0.1f);
    	gameObject.AddComponent(topPanel);

        // Editor View
        CustomButton editorButton = new CustomButton("Editor View");
        editorButton.theme = UIManager.themes["BigButtons"];
        editorButton.renderLayer = 2;
        editorButton.parent = topPanel.rid;
        editorButton.position = new UDim2(0, 0, 8, 8);
        editorButton.size = new UDim2(0.25f, 1, -8, -16);
        editorButton.buttonPressed += SwitchView;
        gameObject.AddComponent(editorButton);

        UILabel editorLabel = new UILabel();
        editorLabel.theme = UIManager.themes["BigButtons"];
        editorLabel.renderLayer = 3;
        editorLabel.parent = editorButton.rid;
        editorLabel.position = new UDim2(0.05f,0.05f,8,8);
        editorLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        editorLabel.text = "Editor View";
        editorLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(editorLabel);

        // Resources View
        CustomButton resourcesView = new CustomButton("Resource View");
        resourcesView.theme = UIManager.themes["BigButtons"];
        resourcesView.renderLayer = 2;
        resourcesView.parent = topPanel.rid;
        resourcesView.position = new UDim2(0.25f, 0, 4, 8);
        resourcesView.size = new UDim2(0.25f, 1, -4, -16);
        resourcesView.buttonPressed += SwitchView;
        gameObject.AddComponent(resourcesView);

        UILabel resourcesLabel = new UILabel();
        resourcesLabel.theme = UIManager.themes["BigButtons"];
        resourcesLabel.renderLayer = 3;
        resourcesLabel.parent = resourcesView.rid;
        resourcesLabel.position = new UDim2(0.05f,0.05f,8,8);
        resourcesLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        resourcesLabel.text = "Resources";
        resourcesLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(resourcesLabel);

        // Plugin View
        CustomButton pluginButton = new CustomButton("Plugin View");
        pluginButton.theme = UIManager.themes["BigButtons"];
        pluginButton.renderLayer = 2;
        pluginButton.parent = topPanel.rid;
        pluginButton.position = new UDim2(0.5f, 0, 4, 8);
        pluginButton.size = new UDim2(0.25f, 1, -4, -16);
        pluginButton.buttonPressed += SwitchView;
        gameObject.AddComponent(pluginButton);

        UILabel pluginLabel = new UILabel();
        pluginLabel.theme = UIManager.themes["BigButtons"];
        pluginLabel.renderLayer = 3;
        pluginLabel.parent = pluginButton.rid;
        pluginLabel.position = new UDim2(0.05f,0.05f,8,8);
        pluginLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        pluginLabel.text = "Plugins";
        pluginLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(pluginLabel);

        // Shaders View
        CustomButton shadersView = new CustomButton("Shader View");
        shadersView.theme = UIManager.themes["BigButtons"];
        shadersView.renderLayer = 2;
        shadersView.parent = topPanel.rid;
        shadersView.position = new UDim2(0.75f, 0, 4, 8);
        shadersView.size = new UDim2(0.25f, 1, -12, -16);
        shadersView.buttonPressed += SwitchView;
        gameObject.AddComponent(shadersView);

        UILabel shadersLabel = new UILabel();
        shadersLabel.theme = UIManager.themes["BigButtons"];
        shadersLabel.renderLayer = 3;
        shadersLabel.parent = shadersView.rid;
        shadersLabel.position = new UDim2(0.05f,0.05f,8,8);
        shadersLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        shadersLabel.text = "Shaders";
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
            case "Resource View":
                currentView = resourceView;
                break;
            case "Shader View":
                currentView = shaderView;
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
        bottomPanel.theme = UIManager.themes["BigPanels"];
    	bottomPanel.parent = editorView.rid;
    	bottomPanel.position = new UDim2(0.2f, 0.7f);
    	bottomPanel.size = new UDim2(0.6f, 0.3f);
    	gameObject.AddComponent(bottomPanel);

        CustomButton createGOButton = new CustomButton();
        createGOButton.theme = UIManager.themes["BigButtons"];
        createGOButton.renderLayer = 2;
        createGOButton.parent = bottomPanel.rid;
        createGOButton.position = new UDim2(0, 0, 10, 10);
        createGOButton.size = new UDim2(0.333333f, 0.5f, -15, -15);
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
        createGOLabel.theme = UIManager.themes["BigButtons"];
        createGOLabel.renderLayer = 3;
        createGOLabel.parent = createGOButton.rid;
        createGOLabel.position = new UDim2(0.05f,0.05f,8,8);
        createGOLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        createGOLabel.text = "Create GameObject";
        createGOLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(createGOLabel);

        CustomButton exportSceneButton = new CustomButton();
        exportSceneButton.theme = UIManager.themes["BigButtons"];
        exportSceneButton.renderLayer = 2;
        exportSceneButton.parent = bottomPanel.rid;
        exportSceneButton.position = new UDim2(0, 0.5f, 10, 0);
        exportSceneButton.size = new UDim2(0.333333f, 0.5f, -15, -10);
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
        exportSceneLabel.theme = UIManager.themes["BigButtons"];
        exportSceneLabel.renderLayer = 3;
        exportSceneLabel.parent = exportSceneButton.rid;
        exportSceneLabel.position = new UDim2(0.05f,0.05f,8,8);
        exportSceneLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        exportSceneLabel.text = "Export Scene";
        exportSceneLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(exportSceneLabel);

        CustomButton importSceneButton = new CustomButton();
        importSceneButton.theme = UIManager.themes["BigButtons"];
        importSceneButton.renderLayer = 2;
        importSceneButton.parent = bottomPanel.rid;
        importSceneButton.position = new UDim2(0.333333f, 0.5f, 0, 0);
        importSceneButton.size = new UDim2(0.333333f, 0.5f, -5, -10);
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
        importSceneLabel.theme = UIManager.themes["BigButtons"];
        importSceneLabel.renderLayer = 3;
        importSceneLabel.parent = importSceneButton.rid;
        importSceneLabel.position = new UDim2(0.05f,0.05f,8,8);
        importSceneLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        importSceneLabel.text = "Import Scene";
        importSceneLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(importSceneLabel);

        CustomButton exportPresetButton = new CustomButton();
        exportPresetButton.theme = UIManager.themes["BigButtons"];
        exportPresetButton.renderLayer = 2;
        exportPresetButton.parent = bottomPanel.rid;
        exportPresetButton.position = new UDim2(0.666666f, 0.5f, 0, 0);
        exportPresetButton.size = new UDim2(0.333333f, 0.5f, -10, -10);
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
        exportPresetLabel.theme = UIManager.themes["BigButtons"];
        exportPresetLabel.renderLayer = 3;
        exportPresetLabel.parent = exportPresetButton.rid;
        exportPresetLabel.position = new UDim2(0.05f,0.05f,8,8);
        exportPresetLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        exportPresetLabel.text = "Export Preset";
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
        importPresetButton.theme = UIManager.themes["BigButtons"];
        importPresetButton.renderLayer = 2;
        importPresetButton.parent = bottomPanel.rid;
        importPresetButton.position = new UDim2(0.666666f, 0f, 0, 10);
        importPresetButton.size = new UDim2(0.333333f, 0.5f, -10, -15);
        importPresetButton.buttonPressed += (b) => {
            importDropUpMenu.visible = !importDropUpMenu.visible;
            importDropUpMenu.active = importDropUpMenu.visible;

            if (importDropUpMenu.visible)
            {
                importDropUpMenu.ClearContents();

                string importPath = "./Export";

                if (Directory.Exists(importPath))
                {
                    foreach (string path in Directory.GetFiles(importPath))
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
        importPresetLabel.theme = UIManager.themes["BigButtons"];
        importPresetLabel.renderLayer = 3;
        importPresetLabel.parent = importPresetButton.rid;
        importPresetLabel.position = new UDim2(0.05f,0.05f,8,8);
        importPresetLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        importPresetLabel.text = "Import Preset";
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
        addCompButton.theme = UIManager.themes["BigButtons"];
        addCompButton.renderLayer = 2;
        addCompButton.parent = bottomPanel.rid;
        addCompButton.position = new UDim2(0.333333f, 0f, 0, 10);
        addCompButton.size = new UDim2(0.333333f, 0.5f, -5, -15);
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
        addCompLabel.theme = UIManager.themes["BigButtons"];
        addCompLabel.renderLayer = 3;
        addCompLabel.parent = addCompButton.rid;
        addCompLabel.position = new UDim2(0.05f,0.05f,8,8);
        addCompLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        addCompLabel.text = "Add Component";
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
		scenePanel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;

    	gameObject.AddComponent(scenePanel);
    }

    public override void OnTick(float dt) { if (PluginManager.instance is not null) { PluginManager.instance.Tick(dt); } }
    public override void Cleanup() {}
}