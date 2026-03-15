using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class DateRequestPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.DBUpdateRequest;
    

    [ProtoMember(1)]
    public DateTime? Date1 { get; set; }
    
    [ProtoMember(2)]
    
    public DateTime? Date2 { get; set; }

    private DateRequestPayload()
    {
        Date1 = null;
    }

    public DateRequestPayload(DateTime date1, DateTime date2)
    {
        Date1 = date1;
        Date2 = date2;
    }
    
    public void Execute(bool isServer, Guid id)
    {
        NetworkManager.Instance.SendToId(id,
            Packet.CreatePacket(new DBResponsePayload(DBInteract.ListBetweenDates(Date1, Date2))).ToBytes());
    }
}