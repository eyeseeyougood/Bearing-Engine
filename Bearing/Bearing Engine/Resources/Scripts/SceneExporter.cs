using Assimp;
using Bearing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public class SceneExporter : Component
{
    public static SceneExporter instance;
    public UITextBox exportPathBox;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        instance = this;

        UIButton exportButton = new UIButton();
        exportButton.renderLayer = -1;
        exportButton.anchor = new Vector2(0.0f, 0.0f);
        exportButton.position = new UDim2(0.2f, 0.0f);
        exportButton.size = new UDim2(0.2f, 0.0f, 0, 50);
        exportButton.buttonPressed += SceneExporterButtonPressed;
        gameObject.AddComponent(exportButton);

        UILabel exportLabel = new UILabel();
        exportLabel.renderLayer = 0;
        exportLabel.anchor = new Vector2(0.5f, 0.5f);
        exportLabel.position = new UDim2(0.5f, 0.5f);
        exportLabel.size = new UDim2(1f, 1f, -20, -20);
        exportLabel.text = "Export Scene";
        exportLabel.parent = exportButton.rid;
        gameObject.AddComponent(exportLabel);


        UIButton loadButton = new UIButton();
        loadButton.renderLayer = -1;
        loadButton.anchor = new Vector2(0.0f, 0.0f);
        loadButton.position = new UDim2(0.2f, 0.0f, 0, 50);
        loadButton.size = new UDim2(0.2f, 0.0f, 0, 50);
        loadButton.buttonPressed += SceneExporterLoadPressed;
        gameObject.AddComponent(loadButton);

        UILabel loadLabel = new UILabel();
        loadLabel.renderLayer = 0;
        loadLabel.anchor = new Vector2(0.5f, 0.5f);
        loadLabel.position = new UDim2(0.5f, 0.5f);
        loadLabel.size = new UDim2(1f, 1f, -20, -20);
        loadLabel.text = "Load Scene";
        loadLabel.parent = loadButton.rid;
        gameObject.AddComponent(loadLabel);

        exportPathBox = new UITextBox();
        exportPathBox.anchor = new Vector2(0.0f, 0f);
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
        PhysicsManager.simulating = false;

        string path = exportPathBox.text;

        if (!Directory.Exists(path) || !File.Exists($"{path}/main.json2"))
        {
            Logger.LogError("Cannot load due to invalid path!");
            Logger.LogError("Usage: The path of the folder containing your main.json2!");
            return;
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

        LightManager.lights.Clear();

        // load all of the objects from the exported scene

        GameObject nRoot = SceneLoader.LegacyLoadFromRealFile($"{path}/main.json2");

        foreach (GameObject go in nRoot.immediateChildren.ToList())
        {
            go.parent = Game.instance.root;
        }

        nRoot.Cleanup();

        Hierarchy.instance.UpdateView();
        Inspector.instance.UpdateView();

        Delay(()=>{
            PhysicsManager.simulating = true;
        },0.5f);
    }

    public override void OnTick(float dt)
    {
    }

    public void ExportScene()
    {
        // remove all plugins so their objects dont save
        PluginManager.DisableAll(true);

        // proceed to exporting
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
                        new TransformConverter(),
                        new ShaderParamConverter(),
                        new Vector4Converter(),
                        new Vector3Converter(),
                        new Vector2Converter(),
                        new ColliderConverter(),
                        new RBConverter(),
                        new ComponentConverter(),
                        new GameObjectConverter(),
                        new MeshConverter(),
                        new ShaderConverter(),
                    }
                }).Serialize(jw, Game.instance.root);
                f = sw.ToString();
            }
        }

        File.WriteAllText($"{path}/main.json2", f);

        // re-enable the plugins that where enabled
        PluginManager.EnableAll(true);
    }
}