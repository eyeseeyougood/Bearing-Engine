using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;
using OpenTK.Mathematics;

public class TestComponent : Component
{
    public string objectName { get; set; } = "";

    [HideFromInspector]
    public Vector3 SomeVector { get; set; } = Vector3.Zero;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        Gizmos.CreateSphere(gameObject.transform.position, 0.1f, 10000f);

        objectName = gameObject.name;
        prevVal = objectName;
        
        Logger.Log("Added TestComponent!!!", ConsoleColor.Yellow);
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