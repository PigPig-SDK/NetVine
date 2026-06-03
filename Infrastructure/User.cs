using Infrastructure.Networking.Packets;
namespace Infrastructure;

public class User
{
    public string Username { get; set; } = "Unknown";
    public bool IsOnline => ConnectedUserInfo.IsUserConnected(Username);
    public bool IsHost { get; set; }
    public string IpAddress { get; set; }
    public User() {
        IpAddress = "Unknown";
    }
    public User(string username, bool isHost, string ipAddress)
    {
        Username = username;
        IsHost = isHost;
        IpAddress = ipAddress;
    }
}
