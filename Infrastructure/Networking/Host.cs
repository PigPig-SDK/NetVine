using NetCoreServer;
using System.Net;
using System.Net.Sockets;

namespace Infrastructure.Networking;

public class Host : SslServer
{
    public Host(SslContext context, IPAddress address, int port) : base(context, address, port) { }

    protected override SslSession CreateSession() => new HostSession(this);

    /// <summary>
    /// Override used to prevent sending unauthenticated users packets
    /// </summary>
    public override bool Multicast(ReadOnlySpan<byte> buffer)
    {
        if (!IsStarted)
        {
            return false;
        }

        if (buffer.IsEmpty)
        {
            return true;
        }

        foreach (SslSession value in Sessions.Values)
        {
            if(value is HostSession hostSession)
            {
                if (hostSession.IsPasswordAccepted == false)//Don't send data to people who might listen without our acceptance.
                    continue;
            }

            value.SendAsync(buffer);
        }

        return true;
    }

    protected override void OnError(SocketError error)
    {
        base.OnError(error);
    }
}
