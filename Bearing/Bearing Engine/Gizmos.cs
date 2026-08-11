using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Bearing;

public static class Gizmos
{
    private struct SphereGizmo
    {
        public Vector3 pos = Vector3.Zero;
        public float radius = 1.0f;
        public float time = 0.016f;
        public BearingColour colour = BearingColour.White;

        public SphereGizmo() {}
    }

    private struct VectorGizmo
    {
        public Vector3 pos = Vector3.Zero;
        public Vector3 vector = Vector3.Zero;
        public float time = 0.016f;
        public BearingColour colour = BearingColour.White;

        public VectorGizmo() {}
    }

    //private static List<(Renderable,float)> gizmos = new List<(Renderable, float)>();
    //private static Dictionary<Renderable,GameObject> objects = new Dictionary<Renderable,GameObject>();

    private static MeshRenderer debugSphere;
    private static MeshRenderer debugVector;
    private static List<SphereGizmo> sphereGizmos = new List<SphereGizmo>();
    private static List<VectorGizmo> vectorGizmos = new List<VectorGizmo>();

    private static Material gizmoMaterial = new Material();
/*
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
    }*/

    public static void CreateSphere(Vector3 center, float radius = 1f, float time = 0, BearingColour colour = default)
    {
        SphereGizmo newSphere = new SphereGizmo();
        newSphere.pos = center;
        newSphere.radius = radius;
        newSphere.time = Time.now + time;
        if (colour != default)
            newSphere.colour = colour;

        sphereGizmos.Add(newSphere);
    }

/*
    public static void CreateVector(Vector3 vector, Vector3 center = default, float time = 0, BearingColour colour = default)
    {
        string meshName = "eng/SBP.obj";

        if (sbp == null)
            sbp = new MeshRenderer(meshName);

        MeshRenderer mr = new MeshRenderer(meshName);
        mr.material = gizmoMaterial.Clone();

        BearingColour c = colour;
        if (colour == default)
            c = BearingColour.White;

        mr.material.parameters = new List<ShaderParam>()
        {
            new ShaderParam()
            {
                name = "mainColour",
                value = new List<object> { c.GetZeroToOneA() }
            },
        };

        Vector3 dir = vector.Normalized();
        float length = vector.Length;

        GameObject go = new GameObject();

        ((Transform3D)go.transform).scale = new Vector3(0.01f, length, 0.01f);

        // fix for edge case of unity
        Quaternion rot;
        if (dir == Vector3.UnitY)
        {
            rot = Quaternion.Identity;
        }
        else if (dir == -Vector3.UnitY)
        {
            rot = Quaternion.FromAxisAngle(Vector3.UnitX, MathF.PI);
        }
        else
        {
            Vector3 axis = Vector3.UnitY.Cross(dir).Normalized();
            float angle = MathF.Acos(Vector3.UnitY.Dot(dir));
            rot = Quaternion.FromAxisAngle(axis, angle);
        }

        ((Transform3D)go.transform).qRotation = rot;

        go.components.Add(mr);
        go.Load();

        ((Transform3D)go.transform).position =
            center + dir * (length * 0.5f);

        Game.instance.RemoveRenderable(mr);

        objects.Add(mr, go);
        gizmos.Add((mr, Time.now + time));
    }*/

    public static void CreateVector(Vector3 vector, Vector3 center, float time = 0, BearingColour colour = default)
    {
        VectorGizmo newVector = new VectorGizmo();
        newVector.pos = center;
        newVector.vector = vector;
        newVector.time = Time.now + time;
        if (colour != default)
            newVector.colour = colour;

        vectorGizmos.Add(newVector);
    }

    public static void Init()
    {
        gizmoMaterial.shader = new Shader("eng/default.vert", "eng/default.frag");

        MeshRenderer mr = new MeshRenderer("eng/ICOSphere.obj");
        mr.drawOnTop = true;
        mr.material = gizmoMaterial.Clone();

        mr.material.parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", value = new List<object> { BearingColour.DarkWhite.GetZeroToOneA() } },
        };

        GameObject go = new GameObject();
        go.Load();
        go.AddComponent(mr);

        Game.instance.RemoveRenderable(mr); // prevent this from rendering like normal objects

        debugSphere = mr;


        // vectors
        MeshRenderer mr2 = new MeshRenderer("eng/SBP.obj");
        mr2.drawOnTop = true;
        mr2.material = gizmoMaterial.Clone();

        mr2.material.parameters = new List<ShaderParam>()
        {
            new ShaderParam() { name = "mainColour", value = new List<object> { BearingColour.DarkWhite.GetZeroToOneA() } },
        };

        GameObject go2 = new GameObject();
        go2.components.Add(mr2);
        go2.Load();

        Game.instance.RemoveRenderable(mr2);

        debugVector = mr2;
    }

    public static void Render()
    {
        /*
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
        }*/

        foreach (SphereGizmo gizmo in sphereGizmos.ToList())
        {
            if (gizmo.time <= Time.now)
            {
                sphereGizmos.Remove(gizmo);
                continue;
            }

            debugSphere.gameObject.Transform3D().position = gizmo.pos;
            debugSphere.gameObject.Transform3D().scale = Vector3.One * gizmo.radius;
            debugSphere.material.parameters[0] = new ShaderParam(){name = "mainColour", value = new List<object> { gizmo.colour.GetZeroToOneA() }};

            debugSphere.Render();
        }

        foreach (VectorGizmo gizmo in vectorGizmos.ToList())
        {
            if (gizmo.time <= Time.now)
            {
                vectorGizmos.Remove(gizmo);
                continue;
            }

            debugVector.gameObject.Transform3D().position = gizmo.pos + gizmo.vector / 2f;
            debugVector.gameObject.Transform3D().qRotation = Extensions.LookAt(gizmo.pos, gizmo.pos + gizmo.vector);
            debugVector.gameObject.Transform3D().scale = new Vector3(0.1f, 0.1f, gizmo.vector.LengthFast);
            debugVector.material.parameters[0] = new ShaderParam(){name = "mainColour", value = new List<object> { gizmo.colour.GetZeroToOneA() }};

            debugVector.Render();
        }
    }
}