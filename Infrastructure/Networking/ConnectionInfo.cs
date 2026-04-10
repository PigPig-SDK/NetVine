
using System.Net;
using YamlDotNet.Serialization;

namespace Infrastructure.Networking;

[YamlSerializable]
public record class ConnectionInfo
{
    public string Ip { get; set; }
    public int Port { get; set; }

    public ConnectionInfo()
    {
        Ip = string.Empty;
        Port = default;
    }

    public ConnectionInfo(string connection, int port)
    {
        Ip = connection;
        Port = port;
    }

    public IPAddress? GetIP()
    {
        IPAddress.TryParse(Ip, out IPAddress? ip);
        return ip;
    }
    public static void TryParse(string ip, string port, out ConnectionInfo? connectionInfo)
    {
        ConnectionInfo info = new ConnectionInfo();
        if (!int.TryParse(port, out int portInt))
        {
            connectionInfo = null;
            return;
        }
        info.Ip = ip;
        info.Port = portInt;
        connectionInfo = info;
    }
}