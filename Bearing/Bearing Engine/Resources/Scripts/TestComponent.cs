using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;

public class TestComponent : Component
{
    public string objectName { get; set; } = "";

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        objectName = gameObject.name;
        prevVal = objectName;
        UIManager.currentTheme.buttonHoverAudio = "ButtonEnter.wav";
        UIManager.currentTheme.buttonDownAudio = "ButtonPress.wav";
        UIManager.currentTheme.buttonUpAudio = "ButtonRelease.wav";
    }

    private string prevVal = "";
    public override void OnTick(float dt)
    {
        if (prevVal != objectName)
        {
            gameObject.name = objectName;
            prevVal = objectName;
            Hierarchy.instance.UpdateView();
        } 
    }
}