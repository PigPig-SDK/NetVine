using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class SetLiveViewPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.InitiateLiveView;

    [ProtoMember(1)]
    public bool IsLive { get; set; }

    public SetLiveViewPayload() { }
    public SetLiveViewPayload(bool isLiveViewEnabled)
    {
        IsLive = isLiveViewEnabled;
    }

    public void Execute(bool isServer, Guid id)
    {
        if (isServer) return;//Do not run on server.

        //Start push operations
        if(IsLive)
            NetworkDataManager.Instance.LivePushHostSet.Add(id);
        else
            NetworkDataManager.Instance.LivePushHostSet.Remove(id);
    }
}
