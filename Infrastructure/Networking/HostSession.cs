using NetCoreServer;
using System.Text;

namespace Infrastructure.Networking;

public class HostSession : TcpSession
{
    public HostSession(TcpServer server) : base(server) { }

    protected override void OnConnected()
    {
        Console.WriteLine($"Host session connected: {Id}");
    }
    protected override void OnDisconnected()
    {
        Console.WriteLine($"Host session disconnected: {Id}");
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        Console.WriteLine(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
    }
}
