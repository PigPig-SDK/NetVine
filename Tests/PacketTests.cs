using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests;

public class PacketTests
{
    [Fact]
    public void StringPayload_Serialize_CorrectString()
    {
        byte[] bytes = Packet.CreatePacket(new StringPayload("a")).ToBytes();
        //This is arbitrary...
        Assert.Equal(15, bytes.Length);
    }
    [Fact]
    public void StringPayload_Reserialize_CorrectString()
    {
        byte[] bytes = Packet.CreatePacket(new StringPayload("Hello Testing!")).ToBytes();
        //From bytes to packet
        Packet packet = Packet.FromBytes(bytes);
        StringPayload data = packet.Deserialize<StringPayload>();
        
        Assert.Equal(PacketType.TextMessage, packet.PacketInfo);
        Assert.Equal("Hello Testing!", data.Value);
    }
    [Fact]
    public void StringPayload_ToObject_CorrectObject()
    {
        Packet packet = Packet.CreatePacket(new StringPayload("Hello Testing!"));

        IPacketPayload packetPayload = packet.ToObject();
        Assert.IsType<StringPayload>(packetPayload);
    }
}
