namespace Infrastructure.Networking;

public class ConnectionInfo
{
    public string Connection { get; set; }
    public int Port { get; set; }

    public ConnectionInfo()
    {
        Connection = string.Empty;
        Port = default;
    }

    public ConnectionInfo(string connection, int port)
    {
        Connection = connection;
        Port = port;
    }

}