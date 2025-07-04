using Newtonsoft.Json;
using OpenTK.Mathematics;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing.Multiplayer;

public class SRS : NetModel
{
    private RoomServer roomServer; // used by host
    private Client client;
    
    public bool isHost;
    public bool hasLoaded;

    public override void InitClient(string targetIP)
    {
        client = MultiplayerManager.CreateClient(targetIP);

        hasLoaded = true;
    }

    public override void InitHost()
    {
        roomServer = MultiplayerManager.CreateRoom("");
        client = MultiplayerManager.CreateClient("127.0.0.1:"+roomServer.port);

        isHost = true;
        hasLoaded = true;
    }

    public override void Tick(float delta)
    {
        if (!hasLoaded) return;
        
        client.Update();

        if (!isHost) return;

        // broadcast sync variables
        foreach (SyncVariable @var in syncVars)
        {
            Message m = Message.Create(MessageSendMode.Unreliable, 4); // MsgID:4 means sync variable
            Component comp = GameObject.Find(@var.objID).GetComponent(@var.compID);
            object data = comp.GetType().GetProperty(@var.property).GetValue(comp);

            byte[] bData = (byte[])Extensions.GetExtensionMethod("Serialise"+ data.GetType().Name).Invoke(null, new object[] { data });
            
            m.AddInt(var.objID);
            m.AddInt(var.compID);
            m.AddString(var.property);
            m.AddString(data.GetType().Name);
            m.AddBytes(bData);

            client.Send(m);
        }

        roomServer.Tick(delta);
    }


    // syncing
    List<SyncVariable> syncVars = new List<SyncVariable>();
    public override void AddSyncVariable(int objectID, int compID, string property)
    {
        SyncVariable @var = new SyncVariable()
        {
            objID = objectID,
            compID = compID,
            property = property
        };

        syncVars.Add(@var);
    }

    public override void Broadcast(Message m, ushort ignore)
    {
        roomServer.server.SendToAll(m, ignore);
    }

    [MessageHandler(4)]
    public static void SyncVarsFromClient(ushort client, Message message)
    {
        MultiplayerManager.Broadcast(message, client);
    }

    [MessageHandler(4)]
    public static void SyncVarsFromServer(Message message)
    {
        int objID = message.GetInt();
        int compID = message.GetInt();
        string prop = message.GetString();

        string propType = message.GetString();

        object value = Extensions.GetExtensionMethod("Deserialise" + propType).Invoke(null, new object[] { message.GetBytes() });

        Component c = GameObject.Find(objID).GetComponent(compID);
        c.GetType().GetProperty(prop).SetValue(c, value);
    }
}