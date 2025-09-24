using Bearing;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class CrosshairPlugin : Plugin
{
    private UIElement crosshair;

    public CrosshairPlugin()
    {
        displayName = "Crosshair Plugin";
        description = "Adds a small crosshair to the scene view.";
        author = "eyeseeyougood";
        version = "1.0.0";
        releaseDate = "2025-09-15";
        link = "https://github.com/eyeseeyougood";
    }

    public override void OnLoad()
    {
        base.OnLoad();
    }

    GameObject testMarker;
    Material mat;
    public override void OnEnable()
    {
        base.OnEnable();
        
        testMarker = new GameObject();
        testMarker.name = "TestMarker";
        testMarker.transform.position = Vector3.One;
        testMarker.parent = Game.instance.root;

        MeshRenderer mr = new MeshRenderer("Sphere.obj");
        mat = new Material();
        mr.material = mat;
        mr.material.shader = new Shader("default.vert", "default.frag");
        mr.material.attribs = new List<ShaderAttrib>()
        {
            new ShaderAttrib() { name = "aPosition", size = 3 },
            new ShaderAttrib() { name = "aTexCoord", size = 2 },
            new ShaderAttrib() { name = "aNormal", size = 3 },
        };
        testMarker.AddComponent(mr);

        crosshair = new UIPanel();
        crosshair.anchor = new Vector2(0.5f, 0.5f);
        crosshair.position = new UDim2(0.5f, 0.5f);
        crosshair.size = new UDim2(0,0,3,3);
        crosshair.consumedInputs.Clear();
        Inspector.instance.gameObject.AddComponent(crosshair);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        crosshair.Cleanup();
    }

    protected override void OnUpdate(float dt)
    {
        Gizmos.CreateSphere(Vector3.One * 3, 0.2f);

        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Game.instance.camera;
            Ray ray = new Ray(cam.Position, cam.Front);

            if (Extensions.RayMeshIntersection(testMarker.GetComponent<MeshRenderer>().mesh, testMarker.transform, ray))
            {
                mat.SetShaderParameter(new ShaderParam("mainColour", new Vector4(0, 1, 0, 1)));
            }
            else
            {
                mat.SetShaderParameter(new ShaderParam("mainColour", new Vector4(1, 0, 0, 1)));
            }
        }
    }
}