using Infrastructure.Networking.Packets;
using NetCoreServer;

namespace Infrastructure.Networking;

public class HostSession : TcpSession
{
    public HostSession(TcpServer server) : base(server) { }

    private MessageBuffer _messageBuffer  = new();

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
        _messageBuffer.Append(buffer, offset, size);

        while (_messageBuffer.TryReadPacket(out var packetbytes))
        {
            if(Packet.TryFromBytes(packetbytes!, out Packet? packet))
            {
                Task.Run(() => packet!.TryExecute());
            }
            else
            {
                Console.WriteLine("Malformed packet!");
            }
        }
    }
}
