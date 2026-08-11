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
    public static event Action<ConnectionFailedEventArgs> onClientFailedToConnect = (i)=>{};
    public static event Action<DisconnectedEventArgs> onDisconnected = (i)=>{};
    public static event Action onConnectionSuccess = ()=>{};

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

    public static void ConnectionFailed(object? sender, ConnectionFailedEventArgs e)
    {
        onClientFailedToConnect.Invoke(e);
    }

    public static void ConnectionSuccess(object? sender, EventArgs e)
    {
        onConnectionSuccess.Invoke();
    }

    public static void Disconnected(object? sender, DisconnectedEventArgs e)
    {
        onDisconnected.Invoke(e);
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

        c.ConnectionFailed += MultiplayerManager.ConnectionFailed;
        c.Disconnected += MultiplayerManager.Disconnected;
        c.Connected += MultiplayerManager.ConnectionSuccess;
        
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

    public static void Cleanup()
    {
        if (netModel is not null)
            netModel.Cleanup();
    }
}