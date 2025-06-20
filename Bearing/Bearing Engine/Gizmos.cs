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
    private static List<GameObject> spheres = new List<GameObject>();

    private static Material gizmoMaterial = new Material();

    public static void CreateSphere(Vector3 center, float time)
    {
        MeshRenderer mr = new MeshRenderer("ICOSphere.obj", true);
        mr.material = gizmoMaterial;

        GameObject go = new GameObject();
        go.components.Add(mr);
        go.Load();
        go.transform.position = center;

        Game.instance.RemoveOpaqueRenderable(mr); // prevent this from rendering like normal objects

        spheres.Add(go);
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
        gizmoMaterial.parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", vector4 = new Vector4(0.0f, 0.2f, 0.5f, 1.0f) },
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
        }
    }
}
