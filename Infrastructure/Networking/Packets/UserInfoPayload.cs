using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class UserInfoPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.UserInfo;

    [ProtoMember(1)]
    public string UserName { get; set; }

    public UserInfoPayload() 
    { 
        UserName = string.Empty;
    }

    public UserInfoPayload(string userName)
    {
        UserName = userName;
    }

    public void Execute(bool isServer, Guid id)
    {
        Console.WriteLine($"User online : {UserName}");
        ConnectedUserInfo.AddUserData(id, UserName);
    }
}
