using Core;
using Infrastructure.Networking.Packets;
using NetCoreServer;
using System.Net;
using System.Text;

namespace Infrastructure.Networking;

public class Client : TcpClient
{
    private bool _shutdown = false;
    private MessageBuffer _messageBuffer = new();

    public Client(IPAddress address, int port) : base(address, port) { }

    protected override void OnConnected()
    {
        Console.WriteLine($"Client connected: {Id}");
        SendAsync(Packet.CreatePacket(new UserInfoPayload(SystemHistory.Instance.SystemName)).ToBytes());
    }

    override protected void OnDisconnected()
    {
        ConnectedUserInfo.RemoveUserData(Id, out string? username);
        Console.WriteLine($"Client disconnected: {Id} {username}");
    }

    override protected void OnReceived(byte[] buffer, long offset, long size)
    {
        _messageBuffer.Append(buffer, offset, size);

        while (_messageBuffer.TryReadPacket(out var packetbytes))
        {
            if (Packet.TryFromBytes(packetbytes!, out Packet? packet))
            {
                Task.Run(() => packet!.TryExecute(false, Id));
            }
            else
            {
                Console.WriteLine("Malformed packet!");
            }
        }
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
