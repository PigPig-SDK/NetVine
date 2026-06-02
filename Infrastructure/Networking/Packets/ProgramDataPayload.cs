
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
    [ProtoMember(3)]
    public double UsedRamMb { get; set; }

    [ProtoMember(4)]
    public double TotalRamMb { get; set; }

    public void Execute(bool isServer, Guid id)
    {
        if (!isServer) throw new InvalidOperationException("Cannot execute on client! There is a critical error somewhere in your code!");

        if (IsForDatabase)
            DBInteract.Store(ProgramDataArray, false);
        else
            NetworkDataManager.Instance.StoreRemoteMetrics(
                ProgramDataArray.FirstOrDefault()?.SystemName ?? "",
                UsedRamMb,
                TotalRamMb);

        NetworkDataManager.Instance.LiveDataRecieved(ProgramDataArray);
    }

}
