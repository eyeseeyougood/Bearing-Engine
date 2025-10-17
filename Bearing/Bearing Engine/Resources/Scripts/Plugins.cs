using Bearing;
using BulletSharp;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class APIMethod : Attribute { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RequirePlugin : Attribute {
    public string[] requiredPlugins { get; private set; } = new string[1];
    public RequirePlugin(string plugin) { requiredPlugins[0] = plugin; }
    public RequirePlugin(params string[] plugins) { requiredPlugins = plugins; }
}

public partial class Plugin : Component
{
    private Dictionary<string, MethodInfo> api = new Dictionary<string, MethodInfo>();
    private bool enabled;

    public string displayName = "N/A";
    public string description = "N/A";
    public string author = "N/A";
    public string version = "1.0.0";
    public string releaseDate = "N/A";
    public string link = "https://www.example.com";

    public override void OnLoad()
    {
        // caching api methods
        MethodInfo[] methods = GetType().GetMethods();

        foreach (MethodInfo method in methods)
        {
            if (method.IsDefined(typeof(APIMethod), true))
            {
                api.Add(method.Name, method);
            }
        }
    }

    public bool IsEnabled()
    {
        return enabled;
    }

    public virtual void OnEnable()
    {
        enabled = true;

        Logger.Log(displayName + " plugin enabled", ConsoleColor.Blue);
    }

    public virtual void OnDisable()
    {
        enabled = false;

        Logger.Log(displayName + " plugin disabled", ConsoleColor.Blue);
    }

    public object? CallAPI(string funcName, params object?[]? parameters)
    {
        object? result = api[funcName].Invoke(this, parameters);

        return result;
    }

    protected virtual void OnUpdate(float dt)
    {
    }

    public sealed override void OnTick(float dt)
    {
        if (enabled)
            OnUpdate(dt);
    }

    public sealed override void Cleanup()
    {
        OnDisable();
    }
}

public static partial class PluginManager
{
    private static Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();

    public static void InitManager()
    {
        InitUI();
    }

    public static void InitPlugins()
    {
        GameObject pluginHolder = Inspector.instance.gameObject;
        
        Type[] installedPlugins = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Plugin)) && !t.IsAbstract).ToArray();

        List<string> allPluginNames = new List<string>();
        foreach (Type plugin in installedPlugins) { allPluginNames.Add(plugin.Name); }

        foreach (Type plugin in installedPlugins)
        {
            Plugin? instance = (Plugin?)Activator.CreateInstance(plugin);

            if (instance != null)
            {
                // check for requirements
                string[] requiredPlugins = new string[0];
                if (plugin.IsDefined(typeof(RequirePlugin), true))
                {
                    requiredPlugins = plugin.GetCustomAttribute<RequirePlugin>().requiredPlugins;
                }

                foreach (string req in requiredPlugins)
                {
                    if (!allPluginNames.Contains(req))
                    {
                        Logger.LogError($"Failed to load plugin: '{plugin.Name}' due to missing '{req}'");
                        Logger.LogError($"Aborting Plugin Loading");
                        CleanupAll();
                        return;
                    }
                }

                pluginHolder.AddComponent(instance);
                plugins.Add(plugin.Name, instance);
            }
            else
                Logger.LogError($"Failed to instantiate plugin: '{plugin.Name}'");
        }

        EnableAll();
    }

    public static Plugin GetPlugin(string name)
    {
        Plugin result = null;

        if (plugins.ContainsKey(name))
        {
            result = plugins[name];
        }
        else
            Logger.LogError("Couldn't find plugin with name '{name}', try checking the spelling of name or use 'RequirePlugin' attribute for safer loading.");

        return result;
    }

    private static List<string> preferenceCache = new List<string>();
    public static void EnableAll(bool usePreferences = false)
    {
        foreach (var kvp in plugins)
        {
            if (usePreferences)
            {
                if (preferenceCache.Contains(kvp.Key))
                    kvp.Value.OnEnable();
            }
            else
                kvp.Value.OnEnable();
        }
    }

    public static void DisableAll(bool rememberPreferences = false)
    {
        preferenceCache.Clear();

        foreach (var kvp in plugins)
        {
            if (kvp.Value.IsEnabled() && rememberPreferences)
                preferenceCache.Add(kvp.Key);
            kvp.Value.OnDisable();
        }
    }

    public static void EnablePlugin(string name)
    {
        plugins[name].OnEnable();
    }

    public static void DisablePlugin(string name)
    {
        plugins[name].OnDisable();
    }

    private static void CleanupAll()
    {
        foreach (var kvp in plugins)
        {
            kvp.Value.gameObject.RemoveComponent(kvp.Value);
        }

        plugins.Clear();
    }
}

public static partial class PluginManager
{
    private static UIPanel menu;
    private static UIVerticalScrollView scroll;
    private static GameObject uiObject;
    private static UIVerticalScrollView infoMenu;

    private static void InitUI()
    {
        uiObject = Inspector.instance.gameObject;

        // init button

        UIButton button = new UIButton();
        button.renderLayer = -2;
        button.anchor = new Vector2(0.0f, 1.0f);
        button.position = new UDim2(0.2f, 1.0f);
        button.size = new UDim2(0.2f, 0, 0, 100);
        button.buttonPressed += ButtonPressed;
        uiObject.AddComponent(button);

        UILabel label = new UILabel();
        label.renderLayer = 0;
        label.anchor = new Vector2(0.5f, 0.5f);
        label.position = new UDim2(0.5f, 0.5f);
        label.size = new UDim2(1f, 1f, -20, -20);
        label.text = "Plugins";
        label.parent = button.rid;
        uiObject.AddComponent(label);

        // init menu

        menu = new UIPanel();
        menu.anchor = new Vector2(0.5f, 0.5f);
        menu.position = new UDim2(0.5f, 0.5f);
        menu.size = new UDim2(0.5f, 0.5f, 0, 3);
        uiObject.AddComponent(menu);

        // init scroll

        scroll = new UIVerticalScrollView();
        scroll.renderLayer = 1;
        scroll.metadata = new object[] { };
        scroll.anchor = new Vector2(0.0f, 0.0f);
        scroll.position = new UDim2(0.0f, 0.0f);
        scroll.size = new UDim2(1, 1, 0, 0);
        scroll.parent = menu.rid;
        scroll.spacing = 1;
        scroll.scrollByComponents = true;
        scroll.scrollSensitivity = 1;
        scroll.clipContents = true;
        uiObject.AddComponent(scroll);

        // info menu

        infoMenu = new UIVerticalScrollView();
        infoMenu.renderLayer = 11;
        infoMenu.anchor = new Vector2(0.0f, 0.0f);
        infoMenu.position = new UDim2(0.125f, 0.125f);
        infoMenu.size = new UDim2(0.75f, 0.75f, 0, 0);
        infoMenu.themeOverride.verticalScrollBG = BearingColour.LightGray;
        infoMenu.parent = menu.rid;
        infoMenu.spacing = 1;
        infoMenu.scrollByComponents = false;
        infoMenu.scrollSensitivity = 5;
        infoMenu.clipContents = true;
        uiObject.AddComponent(infoMenu);

        HideMenu();
    }

    private static void ButtonPressed(object? sender, EventArgs e)
    {
        ToggleMenu();
    }

    private static void PluginPressed(object? sender, EventArgs e)
    {
        object[] meta = ((UIButton)sender).metadata;
        string plugin = (string)meta[0];

        if (plugins[plugin].IsEnabled())
        {
            plugins[plugin].OnDisable();
        }
        else
        {
            plugins[plugin].OnEnable();
        }

        UpdateView();
    }

    private static void ToggleMenu()
    {
        if (menu.visible)
            HideMenu();
        else
            ShowMenu();
    }

    private static void ShowMenu()
    {
        menu.visible = true;

        UpdateView();

        UIManager.Sort();
    }

    private static void HideMenu()
    {
        menu.visible = false;
        infoMenu.visible = false;
    }

    public static void UpdateView()
    {
        // remove old plugins

        scroll.ClearContents();

        // add new plugins

        string[] pluginList = plugins.Keys.ToArray();

        foreach (string plugin in pluginList)
        {
            UIPanel panel = new UIPanel();
            panel.anchor = new Vector2(0.0f, 0.0f);
            panel.position = new UDim2(0.0f, 0.0f);
            panel.size = new UDim2(0, 0, 0, 100);
            uiObject.AddComponent(panel);

            // plugin button
            UIButton button = new UIButton();
            button.metadata = new object[] { plugin };
            button.renderLayer = 2;
            button.anchor = new Vector2(0.0f, 0.5f);
            button.position = new UDim2(0.0f, 0.5f);
            button.size = new UDim2(0.8f, 1f, 0, 0);
            button.buttonReleased += PluginPressed;
            BearingColour c = plugins[plugin].IsEnabled() ? BearingColour.Green : BearingColour.Red;
            button.themeOverride.buttonUpBackground = c;
            button.themeOverride.buttonDownBackground = c;
            button.themeOverride.buttonHoverBackground = c;
            button.parent = panel.rid;
            uiObject.AddComponent(button);

            UILabel label = new UILabel();
            label.renderLayer = 3;
            label.anchor = new Vector2(0.5f, 0.5f);
            label.position = new UDim2(0.5f, 0.5f);
            label.size = new UDim2(1f, 1f, -20, -20);
            label.text = plugins[plugin].displayName;
            label.parent = button.rid;
            uiObject.AddComponent(label);

            // info button
            UIButton editButton = new UIButton();
            editButton.renderLayer = 3;
            editButton.position = new UDim2(0.8f, 0, 0, 0);
            editButton.size = new UDim2(0.2f, 1f, 0, 0);
            editButton.buttonPressed += OpenPluginInfo;
            editButton.metadata = new object[] { plugin };
            editButton.parent = panel.rid;
            uiObject.AddComponent(editButton);

            UIImage editImage = new UIImage();
            editImage.renderLayer = 6;
            editImage.size = new UDim2(1, 1, 0, 0);
            editImage.parent = editButton.rid;
            uiObject.AddComponent(editImage);
            editImage.SetTexture(Texture.LoadFromFile("./Resources/Textures/Info.png"));

            
            scroll.contents.Add(panel.rid);
        }

        UIManager.Sort();
    }

    public static void UpdateInfoView(string pluginName)
    {
        // remove old info

        infoMenu.ClearContents();

        // add new info

        Plugin plugin = plugins[pluginName];

        CreateInfoLabel(plugin.displayName);
        CreateInfoLabel(plugin.description);
        CreateInfoLabel("By: " + plugin.author);
        CreateInfoLabel(plugin.link);
        CreateInfoLabel("Version: " + plugin.version);
        CreateInfoLabel("Released: " + plugin.releaseDate);

        UIManager.Sort();
    }

    private static void CreateInfoLabel(string text)
    {
        UILabel label = new UILabel();
        label.renderLayer = 12;
        label.anchor = new Vector2(0.0f, 0.0f);
        label.size = new UDim2(0, 0, 0, 100f);
        label.text = text;
        uiObject.AddComponent(label);
        infoMenu.contents.Add(label.rid);
    }

    private static void OpenPluginInfo(object? sender, EventArgs e)
    {
        if (infoMenu.visible)
        {
            infoMenu.visible = false;
            return;
        }
        infoMenu.visible = true;
        UpdateInfoView((string)((UIButton)sender).metadata[0]);
    }
}