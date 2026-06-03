using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class UserInfoPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.UserInfo;

    [ProtoMember(1)]
    public string UserName { get; set; }

    [ProtoMember(2)]
    public string Password { get; set; }

    public bool isHosting = true;

    public UserInfoPayload() 
    { 
        UserName = string.Empty;
        Password = string.Empty;
    }

    public UserInfoPayload(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }
    public async Task AddUser(Guid id)
    {
        var client = NetworkManager.Instance.GuidToClient(id);
        string? ip = client?.Address;
        if(ip is null)
        {
            var hostSession = NetworkManager.Instance.GuidToHostSession(id);
            ip = hostSession?.Ip;
        }

        using var context = new DBInteract();
        context.AddUser(new User(UserName, isHosting, ip ?? "Unknown"));//Try add new user
        await context.SaveChangesAsync();
    }

    public async void Execute(bool isServer, Guid id)
    {
        if (isServer)
            isHosting = false;

        ConnectedUserInfo.AddUserData(id, UserName);
        await AddUser(id);
    }
}
