using Newtonsoft.Json;
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

    private List<(string, string)> networkedObjects = new List<(string, string)>(); // all objects that have been spawned

    public override void InitClient(string targetIP)
    {
        client = MultiplayerManager.CreateClient(targetIP);

        hasLoaded = true;
    }

    public ushort GetClientID()
    {
        if (client == null)
            return 0;

        return client.Id;
    }

    public override void InitHost(ushort port = 2025)
    {
        roomServer = MultiplayerManager.CreateRoom("", port);
        client = MultiplayerManager.CreateClient("127.0.0.1:"+roomServer.port);

        isHost = true;
        hasLoaded = true;
    }

    public override void Tick(float delta)
    {
        if (!hasLoaded) return;
        
        client.Update();

        // broadcast sync variables
        foreach (SyncVariable @var in syncVars)
        {
            Message m = Message.Create(MessageSendMode.Unreliable, 4); // MsgID:4 means sync variable
            Component comp = GameObject.Find(@var.objName).GetComponent(@var.compID);

            if (comp == null) // issue with cleanup probs XDD
                continue;

            object data = comp.GetType().GetProperty(@var.property).GetValue(comp);

            byte[] bData = (byte[])Extensions.GetExtensionMethod("Serialise"+ data.GetType().Name).Invoke(null, new object[] { data });
            
            m.AddString(var.objName);
            m.AddInt(var.compID);
            m.AddString(var.property);
            m.AddString(data.GetType().Name);
            m.AddBytes(bData);

            client.Send(m);
        }

        if (!isHost) return;

        roomServer.Tick(delta);
    }


    // syncing
    List<SyncVariable> syncVars = new List<SyncVariable>();
    public override void AddSyncVariable(string objectName, int compID, string property)
    {
        SyncVariable @var = new SyncVariable()
        {
            objName = objectName,
            compID = compID,
            property = property
        };

        syncVars.Add(@var);
    }

    public override void RemoveSyncVariable(string objectName, int compID, string property)
    {
        SyncVariable @var = new SyncVariable()
        {
            objName = objectName,
            compID = compID,
            property = property
        };

        if (syncVars.Contains(@var))
            syncVars.Remove(@var);
    }

    public override void InstantiateObject(string prefabName, string newName, params string[] instantiationData) // currently slightly limiting since string only serialisable
    {
        Message m = Message.Create(MessageSendMode.Reliable, 3); // MsgID:3 means instantiate object

        m.AddString(prefabName);
        m.AddString(newName);
        m.AddStrings(instantiationData);

        client.Send(m);
    }

    public override void RemoveObject(string name)
    {
        Message m = Message.Create(MessageSendMode.Reliable, 5); // MsgID:5 means remove object

        m.AddString(name);

        client.Send(m);
    }

    public override void Broadcast(Message m, ushort ignore)
    {
        roomServer.server.SendToAll(m, ignore);
    }

    public override void SendToServer(Message m)
    {
        client.Send(m);
    }

    [MessageHandler(4)]
    public static void SyncVarsFromClient(ushort client, Message message)
    {
        MultiplayerManager.Broadcast(message, client);
    }

    [MessageHandler(4)]
    public static void SyncVarsFromServer(Message message)
    {
        string objName = message.GetString();
        int compID = message.GetInt();
        string prop = message.GetString();

        string propType = message.GetString();

        object value = Extensions.GetExtensionMethod("Deserialise" + propType).Invoke(null, new object[] { message.GetBytes() });

        GameObject go = GameObject.Find(objName);
        if (go == null)
            return;

        Component c = go.GetComponent(compID);
        c.GetType().GetProperty(prop).SetValue(c, value);
    }

    [MessageHandler(3)] // MsgID:3 means instantiate an object from file
    public static void InstanceFromClient(ushort client, Message message)
    {
        MultiplayerManager.Broadcast(message, client);
    }

    [MessageHandler(5)] // MsgID:5 means remove networked object from all clients
    public static void DestroyFromClient(ushort client, Message message)
    {
        MultiplayerManager.Broadcast(message, client);
    }

    [MessageHandler(6)] // MsgID:6 means a user-made message, aka whatever the user wants it to be, but it is generally just a msg broadcasted to every client
    public static void UserMadeMessageFromClient(ushort client, Message message)
    {
        MultiplayerManager.Broadcast(message, client);
    }

    [MessageHandler(3)]
    public static void InstanceFromServer(Message message)
    {
        string prefab = message.GetString();
        string newName = message.GetString(); // when instantiated over the network a new name must be sent to avoid issues with syncing later
        string[] instantiationData = message.GetStrings();

        GameObject newObject = SceneLoader.LoadFromFile($"./Resources/Scene/{prefab}.json", false);
        newObject.name = newName;
        newObject.tag = "OtherPlayer";
        newObject.parent = Game.instance.root;
        newObject.metadata = instantiationData;
        newObject.Load();
    }

    [MessageHandler(5)]
    public static void DestroyFromServer(Message message)
    {
        string name = message.GetString(); // the name given here is the same name that every client has

        GameObject? go = GameObject.Find(name);
        if (go != null)
            go?.Cleanup();
    }

    [MessageHandler(6)] // MsgID:6 means a user-made message, aka whatever the user wants it to be, but it is generally just a msg broadcasted to every client
    public static void UserMadeMessage(Message message)
    {
        MultiplayerManager.MessageReceived(message);
    }
}