using NetCoreServer;
using System.Net;
using System.Net.Sockets;

namespace Infrastructure.Networking;

public class Host : TcpServer
{
    public Host(IPAddress address, int port) : base(address, port) { }

    protected override TcpSession CreateSession() => new HostSession(this);

    protected override void OnError(SocketError error)
    {
        base.OnError(error);
    }
}
