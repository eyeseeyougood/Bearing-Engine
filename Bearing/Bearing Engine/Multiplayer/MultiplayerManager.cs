using Riptide;
using Riptide.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing.Multiplayer;

/// <summary>
/// This class simply handles the basic setup of multiplayer stuff such as instantiating room servers and clients
/// </summary>
public static class MultiplayerManager
{
    public static bool isMultiplayer { get; private set; }
    public static bool isHost { get; private set; }

    public static event Action<Message> onMessageRecieved = (i)=>{}; // this does not fire every single time a message is received, this is for user-made msgs

    private static Dictionary<string, string> settings = new Dictionary<string, string>();

    private static NetModel netModel;

    public static void Init()
    {
        // settings
        Dictionary<string, string>? _settings = SceneSettingsManager.GetSettings("Multiplayer");

        if (_settings == null)
        {
            Logger.LogError("Invalid Multiplayer Settings!");
            return;
        }

        settings = _settings;

        // check if to use continue with multiplayer setup
        if (!bool.Parse(settings["isMultiplayer"])) return;

        isMultiplayer = true;

        RiptideLogger.Initialize(Console.WriteLine, true);

        // network model
        NetworkModel model = (NetworkModel)Enum.Parse(typeof(NetworkModel), settings["netModel"]);

        SetupNetworkModel(model);

        Logger.Log("Multiplayer initialised");
    }

    public static NetModel GetNetModel()
    {
        return netModel;
    }

    public static void MessageReceived(Message m)
    {
        onMessageRecieved.Invoke(m);
    }

    public static void InstantiateObject(string prefabName, string newName, params string[] instantiationData)
    {
        netModel.InstantiateObject(prefabName, newName, instantiationData);
    }

    public static void RemoveObject(string name)
    {
        netModel.RemoveObject(name);
    }

    public static void AddSyncVariable(string objName, int compID, string property)
    {
        netModel.AddSyncVariable(objName, compID, property);
    }

    public static void RemoveSyncVariable(string objName, int compID, string property)
    {
        netModel.RemoveSyncVariable(objName, compID, property);
    }

    public static void Broadcast(Message m, ushort ignoreClient = 0)
    {
        netModel.Broadcast(m, ignoreClient);
    }

    public static void SendToServer(Message m)
    {
        netModel.SendToServer(m);
    }

    public static void Tick(float delta)
    {
        if (isMultiplayer)
            netModel.Tick(delta);
    }

    public static void InitHost(ushort port)
    {
        netModel.InitHost(port);
    }

    public static void InitClient(string targetIP)
    {
        netModel.InitClient(targetIP);
    }

    public static Client CreateClient(string address)
    {
        Client c = new Client();
        c.Connect(address);

        return c;
    }

    private static void SetupNetworkModel(NetworkModel model)
    {
        netModel = (NetModel)Activator.CreateInstance(Type.GetType("Bearing.Multiplayer." + Enum.GetName(model)));
    }

    public static RoomServer CreateRoom(string roomName, ushort port = 2025)
    {
        RoomServer roomServer = new RoomServer();
        roomServer.maxCapacity = ushort.Parse(settings["maxPlayers"]);
        roomServer.name = roomName;
        roomServer.port = port;
        roomServer.Init();

        return roomServer;
    }
}