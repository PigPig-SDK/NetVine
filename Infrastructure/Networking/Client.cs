using Core;
using Infrastructure.Networking.Packets;
using NetCoreServer;
using System.Net;
using System.Text;

namespace Infrastructure.Networking;

public class Client : SslClient
{
    private bool _shutdown = false;
    private MessageBuffer _messageBuffer = new();
    public ConnectionInfo ConnectionInfo { get; private set; }
    
    public Client(SslContext context, IPAddress address, int port, string password, ConnectionInfo connectionInfo) : base(context, address, port)
    {
        ConnectionInfo = connectionInfo;
    }

    protected override void OnHandshaked()
    {
        Console.WriteLine($"Client connected: {Id}");
        SendAsync(Packet.CreatePacket(new UserInfoPayload(SystemHistory.Instance.SystemName, ConnectionInfo.Password)).ToBytes());
    }
    protected override void OnConnected()
    {
        NetworkManager.Instance.OnConnectToHost?.Invoke(Id, ConnectionInfo);
    }
    override protected void OnDisconnected()
    {
        ConnectedUserInfo.RemoveUserData(Id, out string? username);
        NetworkManager.Instance.OnDisconnectFromHost?.Invoke(Id, ConnectionInfo);
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
                Debug.Log("Malformed packet!");
            }
        }
    }

    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        Console.WriteLine($"Client error: {Id} - {error}");
        NetworkManager.Instance.OnSocketError?.Invoke(Id, ConnectionInfo, error);
    }

    public void DisconnectShutdown()
    {
        _shutdown = true;
        DisconnectAsync();
        while(IsConnected)
            Thread.Yield();//This Yield shouldn't take that long.
    }
}
