using System.Net;

namespace Infrastructure.Networking;

public class NetworkManager
{
    public static NetworkManager _instance = null!;
    public static NetworkManager Instance => _instance ??= new NetworkManager();

    public const int DefaultPort = 12345;
    public const string DefaultHost = "127.0.0.1";

    public List<Client> Clients { get; private set; } = [];



    NetworkManager()
    {
        StartClientConnections();
    }

    private void StartClientConnections()
    {
        foreach (var connectionContext in ConfigManager.ClientConnections)
        {
            IPAddress.TryParse(connectionContext.connection, out IPAddress? ip);
            if (ip == null)
            {
                Console.WriteLine($"Invalid IP address: {connectionContext.connection}");
                continue;
            }
            Client dc = new(ip, connectionContext.port);
            dc.ConnectAsync();
        }
    }

    private void Shutdown()
    {
        foreach (var client in Clients)
        {
            client.DisconnectShutdown();
        }
    }
}
