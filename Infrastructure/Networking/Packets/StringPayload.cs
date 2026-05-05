using Infrastructure.Notifications;
using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class StringPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.TextMessage;

    [ProtoMember(1)]
    public string Value { get; set; }
    [ProtoMember(2)]
    public NotificationPriority Notification = NotificationPriority.Message;

    private StringPayload() 
    {
        Value = string.Empty;
    }

    public StringPayload(string value)
    {
        Value = value;
    }

    public void Execute(bool isServer, Guid id)
    {
        NotificationManager.WriteNotification(new(Notification, Value));
        //Relay to other clients.
        if(isServer)
        {
            var data = Packet.CreatePacket(this).ToBytes();
            NetworkManager.Instance.Host?.Multicast(data);
        }
    }
}
