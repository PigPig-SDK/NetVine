using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace Infrastructure;

public class User : DBEntry
{
    public string Username { get; set; } = "Unknown";
    public bool IsOnline => ConnectedUserInfo.IsUserConnected(Username);
    public DateTime LastDateConnected { get; set; }

    public User() { }
    public User(string username, DateTime? lastDate = null)
    {
        Username = username;
        LastDateConnected = lastDate ?? DateTime.Now;
    }
}
