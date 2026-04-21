using Infrastructure.Networking.Packets;
using NetCoreServer;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.Networking;

public class NetworkManager
{
    private static NetworkManager _instance = null!;
    public static NetworkManager Instance => _instance ?? throw new InvalidOperationException($"Call {nameof(SetupInstance)} before accessing instance!!");

    private Timer _timer;
    public const int DefaultPort = 54236;
    public const string DefaultHost = "0.0.0.0";

    private static readonly X509Certificate2 ServerCertificate = GenerateSelfSignedCertificate();
    private static readonly X509Certificate2 ClientCertificate = GenerateSelfSignedCertificate();

    public Dictionary<(IPAddress connection, int port), Client> EstablishedClientConnections { get; private set; } = [];
    public Host? Host { get; private set; }

    /// <summary>
    /// Called when a client loses their connection with the host.
    /// </summary>
    public Action<Guid, ConnectionInfo>? OnDisconnectFromHost;
    public Action<Guid, ConnectionInfo, SocketError>? OnSocketError;
    public Action<Guid, ConnectionInfo>? OnConnectToHost;
    public Action<Guid, NetworkErrorType>? OnNetworkError; 

    public static void SetupInstance()
    {
        if (_instance != null) throw new InvalidOperationException($"Cannot call {nameof(SetupInstance)} more than once!");
        _instance = new NetworkManager();
        ConfigManager.OnClientConnectionAdded += _instance.OnAddClientConnection;
        ConfigManager.OnClientConnectionRemoved += _instance.OnRemoveClientConnection;
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
                case SettingInt.NetworkDisabled:
                    if (ConfigManager.ReadSettingBool(SettingInt.NetworkDisabled))
                        Disconnect();
                    else
                    {
                        if(Host is null) StartHost();

                        RefreshClientConnections();
                    }
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
    public static void AddClientConnection(ConnectionInfo info) { 
        ConfigManager.AddClientConnection(info);
        ConfigManager.TrySaveToFile();
    }
    public static void RemoveClientConnection(ConnectionInfo info)
    {
        ConfigManager.RemoveClientConnection(info);
        ConfigManager.TrySaveToFile();
    }
    private void OnAddClientConnection(ConnectionInfo info) => RefreshClientConnections();
    private void OnRemoveClientConnection(ConnectionInfo info)
    {
        IPAddress? ip = info.GetIP();
        if(ip is null) return;

        (IPAddress, int) infoTuple = (ip, info.Port);

        if (!EstablishedClientConnections.TryGetValue(infoTuple, out Client? client)) return;
        //Shutdown the client connection!
        if(client.IsConnected) client?.Disconnect();
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
        if (!ConfigManager.ReadSettingBool(SettingInt.IsHosting)) return;
        if (ConfigManager.ReadSettingBool(SettingInt.NetworkDisabled)) return;

        var context = new SslContext(SslProtocols.Tls12, ServerCertificate, (sender, certificate, chain, sslPolicyErrors) => true);
        Host = new Host(context, IPAddress.Any, ConfigManager.ReadSetting(SettingInt.HostPort));
        Host.Start();

    }

    public void RefreshClientConnections()
    {
        if (ConfigManager.ReadSettingBool(SettingInt.NetworkDisabled)) return;

        foreach (ConnectionInfo connectionContext in ConfigManager.CurrentClientConnections)
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
                var context = new SslContext(SslProtocols.Tls12, ClientCertificate, (sender, certificate, chain, sslPolicyErrors) => true);
                Client client = new(context, ip, connectionContext.Port, connectionContext.Password, connectionContext);
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

    public Client? GuidToClient(Guid id)
    {
        foreach (Client client in EstablishedClientConnections.Values)
        {
            if (client.Id == id) return client;
        }
        return null;
    }

    public bool IsOnline(ConnectionInfo connectionInfo)
    {
        foreach (Client info in EstablishedClientConnections.Values)
        {
            if (info.ConnectionInfo == connectionInfo) return info.IsConnected;
        }

        return false;
    }
    ~NetworkManager()
    {
        Disconnect();
        ConfigManager.OnClientConnectionAdded -= _instance.OnAddClientConnection;
    }

    public static X509Certificate2 GenerateSelfSignedCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("cn=netvine", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        return new X509Certificate2(cert.Export(X509ContentType.Pfx));
    }
}
