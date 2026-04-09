using NetCoreServer;
using System.Net;
using System.Net.Sockets;

namespace Infrastructure.Networking;

public class Host : SslServer
{
    public Host(SslContext context, IPAddress address, int port) : base(context, address, port) { }

    protected override SslSession CreateSession() => new HostSession(this);

    protected override void OnError(SocketError error)
    {
        base.OnError(error);
    }
}
