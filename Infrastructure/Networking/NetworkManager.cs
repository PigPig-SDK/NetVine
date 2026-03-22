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

    public static void SetupInstance()
    {
        if (_instance != null) throw new InvalidOperationException($"Cannot call {nameof(SetupInstance)} more than once!");
        _instance = new NetworkManager();
        ConfigManager.OnClientConnectionAdded += _instance.OnAddClientConnection;
    }

    private void OnAddClientConnection(ConnectionInfo info) => RefreshClientConnections();//Lazy, but efficent.

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
        if(ConfigManager.ReadSetting(SettingInt.IsHosting) == 0) return;

        Host = new Host(IPAddress.Any, ConfigManager.ReadSetting(SettingInt.HostPort));
        Host.Start();

        Console.WriteLine($"Accepting : {Host.IsAccepting}");
    }

    public void RefreshClientConnections()
    {
        foreach (var connectionContext in ConfigManager.ClientConnections)
        {
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

    public void SendToId(Guid id, byte[] bytes)
    {
        var session = Host?.FindSession(id)?.Send(bytes);

        foreach (var connectionContext in EstablishedClientConnections.Values)
        {
            if (connectionContext.Id == id)
                connectionContext.Send(bytes);
        }
    }

    public void Disconnect()
    {
        //Disconnect from others...
        foreach (var client in EstablishedClientConnections.Values)
        {
            client.DisconnectShutdown();
        }

        Host?.DisconnectAll();
        Host?.Dispose();
    }

    ~NetworkManager()
    {
        Disconnect();
        ConfigManager.OnClientConnectionAdded -= _instance.OnAddClientConnection;
    }
}
