using Core;
using Infrastructure.Networking.Packets;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NetCoreServer;

namespace Infrastructure.Networking;

public class HostSession : SslSession
{
    public HostSession(SslServer server) : base(server) { }

    private MessageBuffer _messageBuffer  = new();

    protected override void OnConnected()
    {
        Console.WriteLine($"Host session connected: {Id}");

        SendAsync(Packet.CreatePacket(new UserInfoPayload(SystemHistory.Instance.SystemName)).ToBytes());

        //Send(Packet.CreatePacket(new DateRequestPayload(DateTime.Now, DateTime.Now.AddSeconds(1))).ToBytes());
    }
    protected override void OnDisconnected()
    {
        ConnectedUserInfo.RemoveUserData(Id, out string? username);
        Console.WriteLine($"Host session disconnected: {Id} {username}");
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        _messageBuffer.Append(buffer, offset, size);

        while (_messageBuffer.TryReadPacket(out var packetbytes))
        {
            if(Packet.TryFromBytes(packetbytes!, out Packet? packet))
            {
                Task.Run(() => packet!.TryExecute(true, Id));
            }
            else
            {
                Console.WriteLine("Malformed packet!");
            }
        }
    }
}
