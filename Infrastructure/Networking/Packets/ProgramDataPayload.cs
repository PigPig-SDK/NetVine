
using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class ProgramDataPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.ProgramData;
    [ProtoMember(1)]
    public bool IsForDatabase { get; set; }
    [ProtoMember(2)]
    public ProgramData[] ProgramDataArray { get; set; } = [];

    public void Execute(bool isServer, Guid id)
    {
        if (!isServer) throw new InvalidOperationException("Cannot execute on client! There is a critical error somewhere in your code!");

        if (IsForDatabase)
            DBInteract.Store(ProgramDataArray);
        else
            NetworkDataManager.Instance.LiveDataRecieved(ProgramDataArray);
    }
}
