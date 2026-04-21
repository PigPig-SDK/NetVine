using CLI;
using Infrastructure;
using Infrastructure.Networking;

namespace CLI.Commands;
public class ConnectCommand : Command
{
    public ConnectCommand(App app) : base(app)
    {
    }

    public override string Name => "connect";

    public override string Description => "Connects to a host";

    public override string Usage => "'connect 192.167.0.4 1000 password'\nConnects to 192.167.0.4 on port 1000 with the password of password";

    public override void Execute(string[] args)
    {
        if(args.Length != 3)
        {
            App.PrintError("Invalid arguments", $"Usage: {Usage}");
            return;
        }

        if(int.TryParse(args[1], out int port) == false || port < 0 || port > 65535)
        {
            App.PrintError("Invalid port", $"Port must be a number between 0 and 65535.");
            return;
        }

        ConnectionInfo info = new ConnectionInfo(
            connection: args[0],
            port, 
            password: args[2]);

        NetworkManager.AddClientConnection(info);
    }
}
