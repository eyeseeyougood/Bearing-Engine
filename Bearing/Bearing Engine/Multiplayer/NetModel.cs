using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing.Multiplayer;

public abstract class NetModel
{
    public abstract void InitHost();
    public abstract void InitClient(string targetIP);
    public abstract void Tick(float delta);
    public abstract void AddSyncVariable(int objectID, int compID, string property);
    public abstract void Broadcast(Message m, ushort ignore);
}