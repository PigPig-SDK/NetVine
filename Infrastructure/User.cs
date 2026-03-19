using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public class User
{
    public string Username { get; set; } = "Unknown";
    public bool IsOnline => ConnectedUserInfo.IsUserConnected(Username);
}
