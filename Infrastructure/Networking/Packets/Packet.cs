using ProtoBuf;
using System.Reflection;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class Packet
{
    [ProtoMember(1)]
    public PacketType PacketInfo { get; set; }
    [ProtoMember(2)]
    public byte[] Data { get; set; }


    private static readonly Dictionary<PacketType, Type> _deserializeMap = new() { { PacketType.TextMessage, typeof(StringPayload)}, { PacketType.DBUpdateRequest, typeof(DateRequestPayload)} };

    private Packet() {
        Data = new byte[0];
    }

    private Packet(PacketType packetType, byte[] data)
    {
        PacketInfo = packetType;
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
    /// Convert this packets payload to an IPacketPayload
    /// </summary>
    public IPacketPayload? ToObject()
    {
        if (!_deserializeMap.TryGetValue(PacketInfo, out Type? type))
            throw new InvalidOperationException($"No deserializer registered for {PacketInfo}");

        MethodInfo method = typeof(Packet)
        .GetMethod(nameof(Deserialize))!
        .MakeGenericMethod(type);

        return (IPacketPayload?)method.Invoke(this, null);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool TryExecute(bool isServer, Guid id)
    {
        try
        {
            IPacketPayload? payload = ToObject();
            if (payload is null) return false;
            payload.Execute(isServer, id);
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is TargetInvocationException || ex is InvalidOperationException)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
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
