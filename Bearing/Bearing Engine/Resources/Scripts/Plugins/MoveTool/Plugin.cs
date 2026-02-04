using Bearing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public partial class MoveToolPlugin : Plugin
{
    private List<GameObject> axes = new List<GameObject>();

    public MoveToolPlugin()
    {
        displayName = "Move Tool";
        description = "Adds a move tool.";
        author = "eyeseeyougood";
        version = "1.0.0";
        releaseDate = "2025-09-15";
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

        GenerateAxis(MovingAxisType.X);
        GenerateAxis(MovingAxisType.Y);
        GenerateAxis(MovingAxisType.Z);
    }

    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);

        if (Input.GetKeyDown(Silk.NET.Input.Key.Number1))
        {
            PluginManager.EnablePlugin("SceneSelectPlugin");
            PluginManager.DisablePlugin("MoveToolPlugin");
        }
        if (Input.GetKeyDown(Silk.NET.Input.Key.Number3))
        {
            PluginManager.EnablePlugin("ScaleToolPlugin");
            PluginManager.DisablePlugin("MoveToolPlugin");
        }
    }

    private void GenerateAxis(MovingAxisType type)
    {
        GameObject axis = new GameObject();
        axis.Load();
        axis.name = "new axis";
        axis.tag = "HierarchyHidden";

        MeshRenderer mr = new MeshRenderer("Cube.obj", true);
        axis.AddComponent(mr);

        MovingAxis ma = new MovingAxis();
        ma.axisToMove = type;
        axis.AddComponent(ma);

        float large = 0.5f;
        float small = 0.05f;
        ((Transform3D)axis.transform).scale = new Vector3(type == MovingAxisType.X ? large : small, type == MovingAxisType.Y ? large : small, type == MovingAxisType.Z ? large : small);
        axis.parent = Game.instance.root;

        axes.Add(axis);
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