using OpenTK.Mathematics;
using Bearing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class SceneSelectPlugin : Plugin
{
    public SceneSelectPlugin()
    {
        displayName = "Scene Select";
        description = "Allows you to click objects in the scene to select them.";
        author = "eyeseeyougood";
        version = "1.0.0";
        releaseDate = "2026-01-27";
        link = "https://github.com/eyeseeyougood";
    }

    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);

        if (Input.GetMouseButtonDown(0) && Input.GetMouseButton(1))
        {
            // form a ray from camera
            Ray ray = new Ray(Game.instance.camera.Position, Game.instance.camera.Front);

            // and intersect with existing function
            GameObject? intersecting = CheckAllMeshes(ray);

            if (intersecting != null)
            {
                GameObject go = intersecting;

                Input.mouseOccupiedBy.Add("selectionTool");
                Hierarchy.instance.selectedObjID = go.id;
                UIButton elem = Hierarchy.instance.GetHierarchyButtonForObject(go);
                Hierarchy.instance.SelectItem(elem, false);
            }
        }
        if (Input.GetMouseButtonUp(0) && Input.mouseOccupiedBy.Contains("selectionTool"))
        {
            Input.mouseOccupiedBy.Remove("selectionTool");
        }

        if (Input.GetKeyDown(Silk.NET.Input.Key.Escape))
        {
            Hierarchy.instance.selectedObjID = -1;
            Hierarchy.instance.selectedID = -1;
            Hierarchy.instance.UpdateView();
        }

        if (Input.GetKeyDown(Silk.NET.Input.Key.Number2))
        {
            PluginManager.EnablePlugin("MoveToolPlugin");
            PluginManager.DisablePlugin("SceneSelectPlugin");
        }
        if (Input.GetKeyDown(Silk.NET.Input.Key.Number3))
        {
            PluginManager.EnablePlugin("ScaleToolPlugin");
            PluginManager.DisablePlugin("SceneSelectPlugin");
        }
    }

    private GameObject? CheckAllMeshes(Ray ray)
    {
        GameObject? result = null;

        foreach (GameObject go in Game.instance.root.immediateChildren)
        {
            if (go.tag == "HierarchyHidden" || go.tag == "EditorObject")
                continue;

            MeshRenderer? mr = go.GetComponent<MeshRenderer>();

            if (mr == null)
                continue;

            if (Extensions.RayMeshIntersection(mr.mesh, ((Transform3D)go.transform), ray))
            {
                result = go;
                break;
            }
        }

        return result;
    }
}