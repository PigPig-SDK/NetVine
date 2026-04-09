
using System.Net;
using YamlDotNet.Serialization;

namespace Infrastructure.Networking;

[YamlSerializable]
public record class ConnectionInfo
{
    public string Ip { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }

    public ConnectionInfo()
    {
        Ip = string.Empty;
        Port = default;
        Password = string.Empty;
    }

    public ConnectionInfo(string connection, int port, string password)
    {
        Ip = connection;
        Port = port;
        Password = password;
    }

    public IPAddress? GetIP()
    {
        IPAddress.TryParse(Ip, out IPAddress? ip);
        return ip;
    }
    public static void TryParse(string ip, string port, string password, out ConnectionInfo? connectionInfo)
    {
        ConnectionInfo info = new ConnectionInfo();
        if (!int.TryParse(port, out int portInt))
        {
            connectionInfo = null;
            return;
        }
        info.Ip = ip;
        info.Port = portInt;
        info.Password = password;
        connectionInfo = info;
    }
}