using Bearing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public partial class LightGizmosPlugin : Plugin
{
    public LightGizmosPlugin()
    {
        displayName = "Light Gizmos";
        description = "Adds a gizmo to every light.";
        author = "eyeseeyougood";
        version = "1.0.0";
        releaseDate = "2026-01-26";
        link = "https://github.com/eyeseeyougood";
    }

    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);

        foreach (Light l in LightManager.lights)
        {
            Vector3 pos = ((Transform3D)l.gameObject.transform).position;
            BearingColour colour = l.colour;
            Gizmos.CreateSphere(pos, 0.1f, 0.02f, colour);
        }
    }
}