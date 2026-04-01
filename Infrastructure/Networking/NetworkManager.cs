using System.Net;

namespace Infrastructure.Networking;

public class NetworkManager
{
    private static NetworkManager _instance = null!;
    public static NetworkManager Instance => _instance ?? throw new InvalidOperationException($"Call {nameof(SetupInstance)} before accessing instance!!");

    private Timer _timer;
    public const int DefaultPort = 54236;
    public const string DefaultHost = "0.0.0.0";

    public Dictionary<(IPAddress connection, int port), Client> EstablishedClientConnections { get; private set; } = [];
    public Host? Host { get; private set; }

    /// <summary>
    /// Called when a client loses their connection with the host.
    /// </summary>
    public Action<Guid> OnDisconnectFromHost;

    public static void SetupInstance()
    {
        if (_instance != null) throw new InvalidOperationException($"Cannot call {nameof(SetupInstance)} more than once!");
        _instance = new NetworkManager();
        ConfigManager.OnClientConnectionAdded += _instance.AddClientConnection;
        ConfigManager.OnClientConnectionRemoved += _instance.RemoveClientConnection;
        ConfigManager.OnSettingChanged += _instance.OnSettingChanged;
    }

    private void OnSettingChanged(Enum setting)
    {
        if (setting is SettingFloat settingFloat)
        {
            switch (settingFloat)
            {
                case SettingFloat.NetworkReconnectInterval:
                    _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds((double)ConfigManager.ReadSetting(SettingFloat.NetworkReconnectInterval)));
                    break;
            }
        }
        else if (setting is SettingInt settingInt)
        {
            switch (settingInt)
            {
                case SettingInt.HostPort:
                    RestartHost();
                    break;
                case SettingInt.IsHosting:
                    if (!ConfigManager.ReadSettingBool(SettingInt.IsHosting))
                        DisconnectHost();
                    else if(Host is null)//We can rehost...
                        StartHost();
                        break;

            }
        }
        else if (setting is SettingString settingString)
        {
            switch(settingString)
            {
                case SettingString.HostIP: 
                    RestartHost(); 
                    break;
            }
        }

    }
    /// <summary>
    /// Disconnects and restarts the hosting process
    /// </summary>
    public void RestartHost()
    {
        DisconnectHost();
        StartHost();
    }
    private void AddClientConnection(ConnectionInfo info) => RefreshClientConnections();
    private void RemoveClientConnection(ConnectionInfo info)
    {
        IPAddress? ip = info.GetIP();
        if(ip is null) return;

        (IPAddress, int) infoTuple = (ip, info.Port);

        if (!EstablishedClientConnections.ContainsKey(infoTuple)) return;
        //Shutdown the client connection!
        EstablishedClientConnections[infoTuple].Disconnect();
        EstablishedClientConnections.Remove(infoTuple);
    }

    private NetworkManager() 
    {
        _timer = new Timer(
            (object? _) => RefreshClientConnections(), 
            null, 
            TimeSpan.Zero,  
            TimeSpan.FromSeconds((double)ConfigManager.ReadSetting(SettingFloat.NetworkReconnectInterval)));
        StartHost();
    }

    public void StartHost()
    {
        if(Host != null) throw new InvalidOperationException($"Cannot host while host is already established!");

        //Don't host!
        if(!ConfigManager.ReadSettingBool(SettingInt.IsHosting)) return;

        Host = new Host(IPAddress.Any, ConfigManager.ReadSetting(SettingInt.HostPort));
        Host.Start();

        Console.WriteLine($"Accepting : {Host.IsAccepting}");
    }

    public void RefreshClientConnections()
    {
        foreach (ConnectionInfo connectionContext in ConfigManager.ClientConnections)
        {
            connectionContext.GetIP();

            IPAddress.TryParse(connectionContext.Ip, out IPAddress? ip);
            if (ip == null)
            {
                Console.WriteLine($"Invalid IP address: {connectionContext.Ip}");
                continue;
            }
            var connectionIdentity = (ip, connectionContext.Port);
            if (EstablishedClientConnections.ContainsKey(connectionIdentity))//Connection has been atempted
            {
                Client client = EstablishedClientConnections[connectionIdentity];
                if (!client.IsConnected && !client.IsConnecting)//Connection is dead
                {
                    client.ConnectAsync();//Retry
                }
            }
            else//No Connection
            {
                Client client = new(ip, connectionContext.Port);
                client.ConnectAsync();
                EstablishedClientConnections.Add(connectionIdentity, client);
            }
        }
    }
    public void SendToAllHosts(byte[] bytes)
    {
        foreach (Client connectionContext in EstablishedClientConnections.Values)
        {
            connectionContext.Send(bytes);
        }
    } 
    public void SendToId(Guid id, byte[] bytes)
    {
        var session = Host?.FindSession(id)?.Send(bytes);

        foreach (var connectionContext in EstablishedClientConnections.Values)
        {
            if (connectionContext.Id == id)
                connectionContext.Send(bytes);
        }
    }
    public void DisconnectHost()
    {
        Host?.DisconnectAll();
        Host?.Dispose();
        Host = null;
    }
    public void Disconnect()
    {
        //Disconnect from others...
        foreach (var client in EstablishedClientConnections.Values)
        {
            client.DisconnectShutdown();
        }
        DisconnectHost();
    }

    ~NetworkManager()
    {
        Disconnect();
        ConfigManager.OnClientConnectionAdded -= _instance.AddClientConnection;
    }
}
