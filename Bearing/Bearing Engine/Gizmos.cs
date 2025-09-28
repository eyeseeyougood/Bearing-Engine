using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class Gizmos
{
    private static List<(IRenderable,float)> gizmos = new List<(IRenderable, float)>();
    private static Dictionary<IRenderable,GameObject> objects = new Dictionary<IRenderable,GameObject>();

    private static Material gizmoMaterial = new Material();

    public static void CreateSphere(Vector3 center, float radius = 1f, float time = 0, BearingColour colour = default)
    {
        MeshRenderer mr = new MeshRenderer("ICOSphere.obj", true);
        mr.material = gizmoMaterial.Clone();
        BearingColour c = colour;
        if (colour == default)
            c = BearingColour.White;

        mr.material.parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", vector4 = c.GetZeroToOneA() },
        };

        GameObject go = new GameObject();
        go.components.Add(mr);
        go.Load();
        go.transform.position = center;
        go.transform.scale = Vector3.One * radius;

        Game.instance.RemoveOpaqueRenderable(mr); // prevent this from rendering like normal objects

        objects.Add(mr, go);
        gizmos.Add((mr, Time.now + time));
    }

    public static void CreateVector(Vector3 vector, Vector3 center = default, float time = 0, BearingColour colour = default)
    {
        MeshRenderer mr = new MeshRenderer("SBP.obj", true);
        mr.material = gizmoMaterial.Clone();
        BearingColour c = colour;
        if (colour == default)
            c = BearingColour.White;

        mr.material.parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", vector4 = c.GetZeroToOneA() },
        };

        GameObject go = new GameObject();
        go.transform.scale = new Vector3(0.02f, vector.Length, 0.02f);
        Vector3 axis = Vector3.Cross(vector.Normalized(), Vector3.UnitY).Normalized();
        float angle = MathF.Acos(Vector3.Dot(vector.Normalized(), Vector3.UnitY));
        if (vector != Vector3.UnitY)
            go.transform.qRotation = Quaternion.FromAxisAngle(axis, -angle);
        go.components.Add(mr);
        go.Load();
        go.transform.position = center + go.transform.GetUp()*vector.Length/2f;

        Game.instance.RemoveOpaqueRenderable(mr); // prevent this from rendering like normal objects

        objects.Add(mr, go);
        gizmos.Add((mr, Time.now + time));
    }

    public static void Init()
    {
        gizmoMaterial.shader = new Shader("default.vert", "default.frag");
        gizmoMaterial.attribs = new List<ShaderAttrib>()
        {
            new ShaderAttrib() { name = "aPosition", size = 3 },
            new ShaderAttrib() { name = "aTexCoord", size = 2 },
            new ShaderAttrib() { name = "aNormal", size = 3 }
        };
    }

    public static void Render()
    {
        List<(IRenderable, float)> remove = new List<(IRenderable, float)>();

        foreach ((IRenderable, float) gizmo in gizmos)
        {
            if (gizmo.Item2 <= Time.now)
            {
                remove.Add(gizmo);
                continue;
            }
            GL.Disable(EnableCap.DepthTest);
            gizmo.Item1.Render();
        }

        foreach ((IRenderable, float) item in remove)
        {
            gizmos.Remove(item);
            objects[item.Item1].Cleanup();
            objects.Remove(item.Item1);
        }
    }
}
