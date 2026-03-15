namespace Infrastructure.Networking.Packets;

public enum PacketType : byte
{
    Unknown,
    TextMessage,
    DBUpdateRequest,
    DBUpdateResponse,
    UserInfo,
}
