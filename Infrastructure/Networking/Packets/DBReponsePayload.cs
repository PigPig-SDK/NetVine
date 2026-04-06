using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class DBResponsePayload : IPacketPayload
{
    public PacketType PacketType => PacketType.DBUpdateResponse;
    

    [ProtoMember(1)]
    public List<ProgramData> DBSection { get; set; }

    private DBResponsePayload()
    {
        DBSection = [];
    }

    public DBResponsePayload(List<ProgramData> dbsection)
    {
        DBSection = dbsection;
    }
    
    public void Execute(bool isServer, Guid id)
    {
        DBInteract.Store(DBSection, false);
    }
    
}