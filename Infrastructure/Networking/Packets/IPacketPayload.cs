namespace Infrastructure.Networking.Packets;
/// <summary>
/// Apply this interface to payload data
/// </summary>
public interface IPacketPayload 
{
    PacketType PacketType { get; }
    /// <summary>
    /// The execution of a payload packet
    /// </summary>
    /// <param name="isServer">If the payload is executed on the server</param>
    /// <param name="id">The ID of the packet sender</param>
    void Execute(bool isServer, Guid id);
}
