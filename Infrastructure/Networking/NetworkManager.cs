using System.Net;

namespace Infrastructure.Networking;

public class NetworkManager
{
    public static NetworkManager _instance = null!;
    public static NetworkManager Instance => _instance ??= new NetworkManager();

    public const int DefaultPort = 12345;
    public const string DefaultHost = "127.0.0.1";

    public Dictionary<(IPAddress connection, int port), Client> EstablishedClientConnections { get; private set; } = [];


    NetworkManager()
    {
        RefreshClientConnections();
    }

    private void RefreshClientConnections()
    {
        foreach (var connectionContext in ConfigManager.ClientConnections)
        {
            IPAddress.TryParse(connectionContext.connection, out IPAddress? ip);
            if (ip == null)
            {
                Console.WriteLine($"Invalid IP address: {connectionContext.connection}");
                continue;
            }
            var connectionIdentity = (ip, connectionContext.port);

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
                Client client = new(ip, connectionContext.port);
                client.ConnectAsync();
                EstablishedClientConnections.Add(connectionIdentity, client);
            }
        }
    }

    private void Shutdown()
    {
        foreach (var client in EstablishedClientConnections.Values)
        {
            client.DisconnectShutdown();
        }
    }
}
