using ProtoBuf;
using System.Net;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class Packet
{
    [ProtoMember(1)]
    public PacketType PacketType { get; set; }
    [ProtoMember(2)]
    public byte[] Data { get; set; }

    private Packet() { }

    private Packet(PacketType packetType, byte[] data)
    {
        PacketType = packetType;
        Data = data;
    }

    public static Packet CreatePacket<T>(T data) where T : IPacketPayload
    {
        using MemoryStream ms = new();
        Serializer.SerializeWithLengthPrefix(ms, data, PrefixStyle.Fixed32);
        return new Packet(data.PacketType, ms.ToArray());
    }
    /// <summary>
    /// Deserialize the payload data as your desired object.
    /// </summary>
    /// <typeparam name="T">T must be a <see cref="IPacketPayload">!</typeparam>
    /// <returns>A deserialized version of T (If it fits)</returns>
    public T Deserialize<T>() where T : IPacketPayload
    {
        using MemoryStream ms = new(Data);
        return Serializer.DeserializeWithLengthPrefix<T>(ms, PrefixStyle.Fixed32);
    }
    /// <summary>
    /// Converts bytes to a packet
    /// </summary>
    /// <param name="bytes">The bytes of the given packet.</param>
    /// <returns>A packet</returns>
    public static Packet FromBytes(byte[] bytes)
    {
        using MemoryStream ms = new(bytes);
        return Serializer.DeserializeWithLengthPrefix<Packet>(ms, PrefixStyle.Fixed32);
    }
    /// <summary>
    /// Trys to generate a packet, catches if the packet is malformed.
    /// </summary>
    /// <param name="bytes">The bytes you are converting to a packet</param>
    /// <param name="packet">A valid packet or null</param>
    /// <returns>True if packet is not null.</returns>
    public static bool TryFromBytes(byte[] bytes, out Packet? packet)
    {
        packet = null;
        try
        {
            packet = FromBytes(bytes);
            return true;
        }
        catch (Exception ex) when (ex is ProtoException || ex is EndOfStreamException || ex is InvalidOperationException) {
        
            Console.WriteLine(ex.ToString());
            return false;
        }
    }
    /// <summary>
    /// This object as a byte array, ready to be networked.
    /// </summary>
    /// <returns>A byte array payload</returns>
    public byte[] ToBytes()
    {
        using MemoryStream ms = new();
        Serializer.SerializeWithLengthPrefix(ms, this, PrefixStyle.Fixed32);
        return ms.ToArray();
    }
}
