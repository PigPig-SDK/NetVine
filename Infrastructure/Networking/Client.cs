using NetCoreServer;
using System.Net;
using System.Text;

namespace Infrastructure.Networking;

public class Client : TcpClient
{
    private bool _shutdown = false;

    public Client(IPAddress address, int port) : base(address, port) { }

    protected override void OnConnected()
    {
        Console.WriteLine($"Client connected: {Id}");
    }

    override protected void OnDisconnected()
    {
        Console.WriteLine($"Client disconnected: {Id}");
    }

    override protected void OnReceived(byte[] buffer, long offset, long size)
    {
        Console.WriteLine(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
    }

    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        Console.WriteLine($"Client error: {Id} - {error}");
    }

    public void DisconnectShutdown()
    {
        _shutdown = true;
        DisconnectAsync();
        while(IsConnected)
            Thread.Yield();//This Yield shouldn't take that long.
    }
}
