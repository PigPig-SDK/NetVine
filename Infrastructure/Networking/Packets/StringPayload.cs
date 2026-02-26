using ProtoBuf;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
}
