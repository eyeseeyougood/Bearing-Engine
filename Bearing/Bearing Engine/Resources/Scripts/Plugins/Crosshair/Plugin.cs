using Bearing;
using OpenTK.Mathematics;
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

    public override void OnEnable()
    {
        base.OnEnable();

        crosshair = new UIPanel();
        crosshair.anchor = new Vector2(0.5f, 0.5f);
        crosshair.position = new UDim2(0.5f, 0.5f);
        crosshair.size = new UDim2(0,0,3,3);
        Inspector.instance.gameObject.AddComponent(crosshair);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        crosshair.Cleanup();
    }
}