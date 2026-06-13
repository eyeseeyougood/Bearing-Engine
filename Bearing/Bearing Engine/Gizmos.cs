using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Bearing;

public static class Gizmos
{
    private static List<(Renderable,float)> gizmos = new List<(Renderable, float)>();
    private static Dictionary<Renderable,GameObject> objects = new Dictionary<Renderable,GameObject>();

    private static Material gizmoMaterial = new Material();

    private static MeshRenderer sbp;

    public static void CreateSphere(Vector3 center, float radius = 1f, float time = 0, BearingColour colour = default)
    {
        MeshRenderer mr = new MeshRenderer("eng/ICOSphere.obj");
        mr.material = gizmoMaterial.Clone();
        BearingColour c = colour;
        if (colour == default)
            c = BearingColour.White;

        mr.material.parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", value = new List<object> { c.GetZeroToOneA() } },
        };

        GameObject go = new GameObject();
        go.components.Add(mr);
        go.Load();
        ((Transform3D)go.transform).position = center;
        ((Transform3D)go.transform).scale = Vector3.One * radius;

        Game.instance.RemoveRenderable(mr); // prevent this from rendering like normal objects

        objects.Add(mr, go);
        gizmos.Add((mr, Time.now + time));
    }

    public static void CreateVector(Vector3 vector, Vector3 center = default, float time = 0, BearingColour colour = default)
    {
        if (sbp == null)
        {
            sbp = new MeshRenderer("eng/SBP.obj");
        }
        MeshRenderer mr = new MeshRenderer(sbp.GetMesh().name);
        mr.material = gizmoMaterial.Clone();
        BearingColour c = colour;
        if (colour == default)
            c = BearingColour.White;

        mr.material.parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", value = new List<object> { c.GetZeroToOneA() } },
        };

        GameObject go = new GameObject();
        ((Transform3D)go.transform).scale = new Vector3(0.02f, vector.Length, 0.02f);
        Vector3 axis = vector.Normalized().Cross(Vector3.UnitY).Normalized();
        float angle = MathF.Acos(vector.Normalized().Dot(Vector3.UnitY));
        if (vector != Vector3.UnitY && vector != -Vector3.UnitY)
            ((Transform3D)go.transform).qRotation = Quaternion.FromAxisAngle(axis, -angle);
        go.components.Add(mr);
        go.Load();
        ((Transform3D)go.transform).position = center + ((Transform3D)go.transform).GetUp()*vector.Length/2f;

        Game.instance.RemoveRenderable(mr); // prevent this from rendering like normal objects

        objects.Add(mr, go);
        gizmos.Add((mr, Time.now + time));
    }

    public static void Init()
    {
        gizmoMaterial.shader = new Shader("eng/default.vert", "eng/default.frag");
    }

    public static void Render()
    {
        List<(Renderable, float)> remove = new List<(Renderable, float)>();

        foreach ((Renderable, float) gizmo in gizmos)
        {
            if (gizmo.Item2 <= Time.now)
            {
                remove.Add(gizmo);
                continue;
            }
            GLContext.gl.Disable(Silk.NET.OpenGL.EnableCap.DepthTest);
            gizmo.Item1.Render();
        }

        foreach ((Renderable, float) item in remove)
        {
            gizmos.Remove(item);
            objects[item.Item1].Cleanup();
            objects.Remove(item.Item1);
        }
    }
}
