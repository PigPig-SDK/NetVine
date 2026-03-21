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
    public async Task AddUser()
    {
        using var context = new DBInteract();
        context.AddUser(new User(UserName));//Try add new user
        await context.SaveChangesAsync();
    }

    public async void Execute(bool isServer, Guid id)
    {
        ConnectedUserInfo.AddUserData(id, UserName);
        await AddUser();
    }
}
