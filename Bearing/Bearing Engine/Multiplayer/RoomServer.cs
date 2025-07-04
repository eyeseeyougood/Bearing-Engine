using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;

namespace Bearing.Multiplayer;

public class RoomServer
{
    public Dictionary<int, string> players = new Dictionary<int, string>();

    public ushort port = 2025;
    public ushort maxCapacity = 10;

    public string name;

    public Server server;

    public RoomServer()
    {
        server = new Server();
    }

    ~RoomServer()
    {
        server.Stop();
    }

    public void Init()
    {
        server.Start(port, maxCapacity);
    }

    public void Tick(float delta)
    {
        server.Update();
    }
}