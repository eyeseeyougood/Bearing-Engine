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
    }

    public static void AddSyncVariable(int objID, int compID, string property)
    {
        netModel.AddSyncVariable(objID, compID, property);
    }

    public static void Broadcast(Message m, ushort ignoreClient = 0)
    {
        netModel.Broadcast(m, ignoreClient);
    }

    public static void Tick(float delta)
    {
        netModel.Tick(delta);
    }

    public static void InitHost()
    {
        netModel.InitHost();
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

    public static RoomServer CreateRoom(string roomName)
    {
        RoomServer roomServer = new RoomServer();
        roomServer.maxCapacity = ushort.Parse(settings["maxPlayers"]);
        roomServer.name = roomName;
        roomServer.Init();

        return roomServer;
    }
}