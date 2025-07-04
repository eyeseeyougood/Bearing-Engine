using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;
using Bearing.Multiplayer;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Launcher : Component
{
    public Vector3 pos { get; set; } = new Vector3(0,0,-3);

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        MultiplayerManager.AddSyncVariable(gameObject.id, id, "pos");
    }

    public override void OnTick(float dt)
    {
        gameObject.transform.position = pos;

        if (Input.GetKeyDown(Keys.H))
            MultiplayerManager.InitHost();
        if (Input.GetKeyDown(Keys.C))
            MultiplayerManager.InitClient("127.0.0.1:2025");
        if (Input.GetKey(Keys.J))
            pos += new Vector3(-dt*5, 0, 0);
        if (Input.GetKey(Keys.L))
            pos += new Vector3(dt * 5, 0, 0);
    }
}
