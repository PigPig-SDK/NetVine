using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class StringPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.TextMessage;

    [ProtoMember(1)]
    public string Value { get; set; }

    private StringPayload() 
    {
        Value = string.Empty;
    }

    public StringPayload(string value)
    {
        Value = value;
    }

    public void Execute()
    {
        Console.WriteLine(Value);
    }
}
