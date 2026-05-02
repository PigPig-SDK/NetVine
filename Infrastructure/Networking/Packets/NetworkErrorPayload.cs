using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class NetworkErrorPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.NetworkError;
    [ProtoMember(1)]
    public NetworkErrorType ErrorType;

    public NetworkErrorPayload()
    {
        ErrorType = NetworkErrorType.Unknown;
    }

    public NetworkErrorPayload(NetworkErrorType errorType)
    {
        ErrorType = errorType;
    }

    public void Execute(bool isServer, Guid id)
    {
        switch (ErrorType)
        {
            case NetworkErrorType.BadPassword:
                {
                    if (isServer) return;
                    NetworkManager.Instance.OnNetworkError?.Invoke(id, ErrorType);
                    break;
                }

        }
    }
}
