using NetCoreServer;
using System.Net;

namespace Infrastructure;

public class DataClient : TcpClient
{
    private bool _shutdown = false;

    public DataClient(IPAddress address, int port) : base(address, port) { }

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
