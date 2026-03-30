using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class SetLiveViewPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.InitiateLiveView;

    [ProtoMember(1)]
    public bool IsLiveViewEnabled { get; set; }

    public SetLiveViewPayload(bool isLiveViewEnabled)
    {
        IsLiveViewEnabled = isLiveViewEnabled;
    }

    public void Execute(bool isServer, Guid id)
    {
        if (isServer) return;//Do not run on server.

        //Start push operations
        NetworkLiveDataManager.Instance.PushHostSet = IsLiveViewEnabled;
    }
}
