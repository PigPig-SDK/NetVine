namespace Infrastructure.Networking.Packets;
/// <summary>
/// Apply this interface to payload data
/// </summary>
public interface IPacketPayload 
{
    PacketType PacketType { get; }
    void Execute();
}
