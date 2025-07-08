using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;

public class MovementTest : Component
{
    private UIElement uiEl;
    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        uiEl = gameObject.GetComponent<UIElement>();
    }

    public override void OnTick(float dt)
    {
        uiEl.position = new UDim2(MathF.Sin(Time.now)*0.5f+0.5f, uiEl.position.scale.Y);
    }
}