using Core;
using Infrastructure.Networking.Packets;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NetCoreServer;

namespace Infrastructure.Networking;

public class HostSession : SslSession
{
    public HostSession(SslServer server) : base(server) { }

    private MessageBuffer _messageBuffer  = new();

    public bool IsPasswordAccepted = false;
    public string Username = string.Empty;
    public string Ip => Socket.RemoteEndPoint?.ToString() ?? "Unknown";

    public static event Action<HostSession>? OnAuthorized;


    protected override void OnHandshaked()
    {
        Debug.Log($"Host session connected: {Id}");
        //Send disregard, as a server dosn't actually care what the client thinks.
        //Possibly codesmell, but it prevents me from writing two different packets for basically the same action.
        SendAsync(Packet.CreatePacket(new UserInfoPayload(SystemHistory.Instance.SystemName,  "Disregard")).ToBytes());
        //Send(Packet.CreatePacket(new DateRequestPayload(DateTime.Now, DateTime.Now.AddSeconds(1))).ToBytes());
        
    }
    protected override void OnDisconnected()
    {
        ConnectedUserInfo.RemoveUserData(Id, out string? username);
        Debug.Log($"Host session disconnected: {Id} {username}");
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        _messageBuffer.Append(buffer, offset, size);

        while (_messageBuffer.TryReadPacket(out var packetbytes))
        {
            if(Packet.TryFromBytes(packetbytes!, out Packet? packet))
            {
                if(packet!.PacketInfo == PacketType.UserInfo)
                {
                    UserInfoPayload userInfo = packet.Deserialize<UserInfoPayload>();
                    Username = userInfo.UserName;

                    if(ConfigManager.ReadSettingBool(SettingInt.UseNetworkPassword))
                    {
                        if(ConfigManager.ReadSetting(SettingString.HostPassword).Equals(userInfo.Password)) IsPasswordAccepted = true;
                    }
                    else
                        IsPasswordAccepted = true;

                    if (IsPasswordAccepted)
                        OnAuthorized?.Invoke(this);
                    else
                    {
                        SendAsync(Packet.CreatePacket(new NetworkErrorPayload(NetworkErrorType.BadPassword)).ToBytes());
                        Disconnect();
                    }

                }
                if(IsPasswordAccepted) Task.Run(() => packet!.TryExecute(true, Id));
            }
            else
            {
                Debug.Log("Malformed packet!");
            }
        }
    }
}
