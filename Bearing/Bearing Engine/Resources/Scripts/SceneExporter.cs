using Bearing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SceneExporter : Component
{
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

        ((UIElement)newUI1).anchor = new OpenTK.Mathematics.Vector2(0.5f, 0f);
        ((UIElement)newUI1).position = new UDim2(0.5f,0f);
        ((UIElement)newUI1).size = new UDim2(0.2f,0f,0f, 100f);
        ((UIElement)newUI1).parent = -1;
        ((UIButton)newUI1).buttonPressed += SceneExporterButtonPressed;

        prefab.Cleanup();
    }

    private void SceneExporterButtonPressed(object? sender, EventArgs e)
    {
        ExportScene();
    }

    public override void OnTick(float dt)
    {
    }

    public void ExportScene()
    {
        if (!Directory.Exists("./Export/"))
            Directory.CreateDirectory("./Export/");

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

        File.WriteAllText("./Export/main.json", f);
    }
}