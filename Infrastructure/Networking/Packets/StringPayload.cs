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
    public NotificationPriority Notification;

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
        Core.Debug.Log($"{id} : {Value}");
        NotificationManager.WriteNotification(new(Notification, Value));//Store outside messages for viewing later.
    }
}
