using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class TestComponent : Component
{
    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        ((UIButton)UIManager.FindFromID(69)).buttonPressed += ButtonPressed;
    }

    private void ButtonPressed(object? sender, EventArgs e)
    {
        Console.WriteLine($"button ({((UIButton)sender).id}) clicked!");
    }

    public override void OnTick(float dt)
    {
    }
}