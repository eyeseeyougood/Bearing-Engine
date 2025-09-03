using Bearing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SceneExporter : Component
{
    private UITextBox exportPathBox;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
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
        ((UIElement)newUI2).rid = UIManager.GetUniqueUIID();

        ((UILabel)newUI2).text = "Export Scene";
        ((UILabel)newUI2).parent = ((UIElement)newUI1).rid;

        ((UIElement)newUI1).anchor = new OpenTK.Mathematics.Vector2(0.0f, 0f);
        ((UIElement)newUI1).position = new UDim2(0.2f,0f);
        ((UIElement)newUI1).size = new UDim2(0.2f,0f,0f, 50f);
        ((UIElement)newUI1).parent = -1;
        ((UIButton)newUI1).buttonPressed += SceneExporterButtonPressed;

        prefab.Cleanup();


        prefab = SceneLoader.LoadFromFile("./Resources/Scene/buttonObject.json", true);
        newUI1 = prefab.GetComponent(0);
        newUI2 = prefab.GetComponent(1);
        prefab.RemoveComponent(newUI1, false);
        prefab.RemoveComponent(newUI2, false);

        gameObject.AddComponent(newUI1);
        gameObject.AddComponent(newUI2);
        newUI1.OnLoad();
        newUI2.OnLoad();

        ((UIElement)newUI1).rid = UIManager.GetUniqueUIID();
        ((UIElement)newUI2).rid = UIManager.GetUniqueUIID();

        ((UILabel)newUI2).text = "Load Scene";
        ((UILabel)newUI2).parent = ((UIElement)newUI1).rid;

        ((UIElement)newUI1).anchor = new OpenTK.Mathematics.Vector2(0.0f, 0f);
        ((UIElement)newUI1).position = new UDim2(0.2f, 0f, 0, 50f);
        ((UIElement)newUI1).size = new UDim2(0.2f, 0f, 0f, 50f);
        ((UIElement)newUI1).parent = -1;
        ((UIButton)newUI1).buttonPressed += SceneExporterLoadPressed;

        prefab.Cleanup();

        exportPathBox = new UITextBox();
        exportPathBox.anchor = new OpenTK.Mathematics.Vector2(0.0f, 0f);
        exportPathBox.position = new UDim2(0.4f, 0);
        exportPathBox.size = new UDim2(0.2f, 0, 0, 100);
        exportPathBox.text = "./Export/";
        gameObject.AddComponent(exportPathBox);
    }

    private void SceneExporterButtonPressed(object? sender, EventArgs e)
    {
        ExportScene();
    }

    private void SceneExporterLoadPressed(object? sender, EventArgs e)
    {
        LoadScene();
    }

    public void LoadScene()
    {
        string path = exportPathBox.text;

        if (!Directory.Exists(path) || !File.Exists($"{path}/main.json"))
        {
            Logger.LogError("Cannot load due to invalid path!");
            Logger.LogError("Usage: The path of the folder containing your main.json!");
        }

        // remove all current objects exept editor objects

        foreach (GameObject go in Game.instance.root.immediateChildren.ToList())
        {
            if (go.tag != "EditorObject")
            {
                Game.instance.root.immediateChildren.Remove(go);
                go.Cleanup();
            }
        }

        // load all of the objects from the exported scene

        GameObject nRoot = SceneLoader.LoadFromRealFile($"{path}/main.json");

        foreach (GameObject go in nRoot.immediateChildren.ToList())
        {
            go.parent = Game.instance.root;
        }

        nRoot.Cleanup();

        Hierarchy.instance.UpdateView();
    }

    public override void OnTick(float dt)
    {
    }

    public void ExportScene()
    {
        string path = exportPathBox.text;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string f = "Failed to parse root!";

        using (var sw = new StringWriter())
        {
            using (var jw = new JsonTextWriter(sw)
            {
                Formatting = Formatting.Indented,
                IndentChar = '\t',
                Indentation = 1,
            })
            {
                JsonSerializer.Create(new JsonSerializerSettings() {
                    Converters = {
                        new ShaderParamConverter(),
                        new Vector4Converter(),
                        new Vector3Converter(),
                        new Vector2Converter(),
                        new ComponentConverter(),
                        new GameObjectConverter(),
                        new MeshConverter(),
                        new ShaderConverter()
                    }
                }).Serialize(jw, Game.instance.root);
                f = sw.ToString();
            }
        }

        File.WriteAllText($"{path}/main.json", f);
    }
}