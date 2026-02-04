using Bearing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public partial class ScaleToolPlugin : Plugin
{
    private List<GameObject> axes = new List<GameObject>();

    public ScaleToolPlugin()
    {
        displayName = "Scale Tool";
        description = "Adds a scale tool.";
        author = "eyeseeyougood";
        version = "1.0.0";
        releaseDate = "2026-01-28";
        link = "https://github.com/eyeseeyougood";
        onByDefault = false;
    }

    public override void OnLoad()
    {
        base.OnLoad();
    }

    public override void OnEnable()
    {
        base.OnEnable();

        GenerateAxis(ScaleAxisType.X);
        GenerateAxis(ScaleAxisType.Y);
        GenerateAxis(ScaleAxisType.Z);
    }

    private void GenerateAxis(ScaleAxisType type)
    {
        GameObject axis = new GameObject();
        axis.Load();
        axis.name = "new axis";
        axis.tag = "HierarchyHidden";

        MeshRenderer mr = new MeshRenderer("ICOSphere.obj", true);
        axis.AddComponent(mr);

        ScaleAxis ma = new ScaleAxis();
        ma.axisToMove = type;
        axis.AddComponent(ma);

        float large = 0.25f;
        ((Transform3D)axis.transform).scale = Vector3.One * large;
        axis.parent = Game.instance.root;
        axes.Add(axis);
    }

    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);

        if (Input.GetKeyDown(Silk.NET.Input.Key.Number1))
        {
            PluginManager.EnablePlugin("SceneSelectPlugin");
            PluginManager.DisablePlugin("ScaleToolPlugin");
        }
        if (Input.GetKeyDown(Silk.NET.Input.Key.Number2))
        {
            PluginManager.EnablePlugin("MoveToolPlugin");
            PluginManager.DisablePlugin("ScaleToolPlugin");
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        foreach (GameObject axis in axes)
        {
            axis.Cleanup();
        }

        axes.Clear();
    }
}