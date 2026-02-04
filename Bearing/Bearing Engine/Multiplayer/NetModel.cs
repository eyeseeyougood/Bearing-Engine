using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing.Multiplayer;

public abstract class NetModel
{
    public abstract void InitHost(ushort port);
    public abstract void InitClient(string targetIP);
    public abstract void Tick(float delta);
    public abstract void AddSyncVariable(string objectName, int compID, string property);
    public abstract void RemoveSyncVariable(string objectName, int compID, string property);
    public abstract void InstantiateObject(string prefabName, string newName, params string[] instantiationData);
    public abstract void RemoveObject(string name);
    public abstract void Broadcast(Message m, ushort ignore);
    public abstract void SendToServer(Message m);
}