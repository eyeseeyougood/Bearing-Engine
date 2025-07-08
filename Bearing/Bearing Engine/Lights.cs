using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class Light : Component
{
    public BearingColour colour { get; set; } = BearingColour.White;

    public override void Cleanup() { }
    public override void OnLoad()
    {
        LightManager.AddLight(this);
    }
    public override void OnTick(float dt) { }
}

public class PointLight : Light
{
    public float intensity { get; set; }
    public float range { get; set; }
}